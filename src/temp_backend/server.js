const express = require('express');
const cors = require('cors');

const app = express();
const port = 3000;

// Enable CORS for Angular frontend
app.use(cors());

// Generate mock accounts and phone numbers
const generateMockAccounts = () => {
  return [
    'ACC_ALPHA123',
    'ACC_BETA456',
    'ACC_GAMMA789',
    'ACC_DELTA012',
    'ACC_OMEGA345'
  ];
};

const generateUniquePhoneNumbers = (count) => {
  const phoneNumbers = new Set();
  while (phoneNumbers.size < count) {
    // Generate a valid US number (format: 1NPANXXXXXX)
    // NPA: 200-999, NXX: 200-999, XXXX: 0000-9999
    const npa = Math.floor(Math.random() * 800 + 200);
    const nxx = Math.floor(Math.random() * 800 + 200);
    const xxxx = Math.floor(Math.random() * 10000).toString().padStart(4, '0');
    phoneNumbers.add(`1${npa}${nxx}${xxxx}`);
  }
  return Array.from(phoneNumbers);
};

// Generate 5000 mock records with realistic distribution
const generateMockData = () => {
  const records = [];
  const today = new Date();
  const accounts = generateMockAccounts();
  const phoneNumbersPerAccount = new Map();
  
  // Generate 100 unique phone numbers for each account
  accounts.forEach(account => {
    phoneNumbersPerAccount.set(account, generateUniquePhoneNumbers(100));
  });
  
  // Generate records for the last 24 hours, in chronological order
  for (let i = 0; i < 5000; i++) {
    const minutesAgo = Math.floor((5000 - i) * (24 * 60) / 5000); // Spread over 24 hours
    const timestamp = new Date(today.getTime() - minutesAgo * 60000);
    
    // Select a random account
    const accountId = accounts[Math.floor(Math.random() * accounts.length)];
    // Select a random phone number from that account's pool
    const phoneNumbers = phoneNumbersPerAccount.get(accountId);
    const phoneNumber = phoneNumbers[Math.floor(Math.random() * phoneNumbers.length)];
    
    records.push({
      id: i + 1,
      requestedAt: timestamp,
      phoneNumber,
      accountId,
      allowed: Math.random() > 0.2, // 80% chance of being allowed
      accountRateHit: Math.random() > 0.9 // 10% chance of rate limit hit
    });
  }
  
  return records;
};

const mockData = generateMockData();

// Generate real-time metrics
const generateMetrics = () => {
  const baseRate = 50; // Base messages per second
  const variance = 20; // Random variance
  return {
    timestamp: new Date(),
    messagesPerSecond: Math.max(0, baseRate + Math.floor(Math.random() * variance * 2) - variance)
  };
};

// Endpoint for real-time metrics - returns last 120 seconds of data
app.get('/api/metrics/realtime', (req, res) => {
  const metrics = [];
  const now = new Date();
  
  // Generate 120 data points, one per second for the last 2 minutes
  for (let i = 119; i >= 0; i--) {
    metrics.push({
      timestamp: new Date(now.getTime() - i * 1000),
      messagesPerSecond: Math.max(0, 50 + Math.floor(Math.random() * 40) - 20)
    });
  }
  
  res.json(metrics);
});

// Endpoint for single metric point - for polling current value
app.get('/api/metrics/current', (req, res) => {
  res.json(generateMetrics());
});

// Endpoint for paginated and filtered history
app.get('/api/history', (req, res) => {
  const page = parseInt(req.query.page) || 1;
  const pageSize = parseInt(req.query.pageSize) || 10;
  const { phoneNumber, startDate, endDate, accountId } = req.query;
  
  // Apply filters
  let filteredRecords = [...mockData];
  
  if (phoneNumber) {
    filteredRecords = filteredRecords.filter(record => 
      record.phoneNumber === phoneNumber
    );
  }
  
  if (accountId) {
    filteredRecords = filteredRecords.filter(record => 
      record.accountId.toLowerCase().includes(accountId.toLowerCase())
    );
  }
  
  if (startDate) {
    const start = new Date(startDate);
    filteredRecords = filteredRecords.filter(record => 
      new Date(record.requestedAt) >= start
    );
  }
  
  if (endDate) {
    const end = new Date(endDate);
    filteredRecords = filteredRecords.filter(record => 
      new Date(record.requestedAt) <= end
    );
  }
  
  const totalRecords = filteredRecords.length;
  const totalPages = Math.ceil(totalRecords / pageSize);
  
  // Apply pagination after filtering
  const startIndex = (page - 1) * pageSize;
  const endIndex = startIndex + pageSize;
  const paginatedRecords = filteredRecords.slice(startIndex, endIndex);
  
  res.json({
    records: paginatedRecords,
    totalRecords,
    totalPages
  });
});

app.listen(port, () => {
  console.log(`Mock server running at http://localhost:${port}`);
}); 