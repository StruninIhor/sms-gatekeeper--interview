import http from 'k6/http';
import { check } from 'k6';

// Function to generate deterministic phone numbers per user
function generatePhoneNumbers(accountId, count) {
    let numbers = [];
    for (let i = 0; i < count; i++) {
        let baseNumber = 1000000000 + (accountId * 100000) + i; 
        numbers.push("1" + baseNumber.toString().padStart(10, "0"));
    }
    return numbers;
}

let users = [];
for (let i = 1; i <= 5; i++) {
    let phoneNumbers = generatePhoneNumbers(i, 20);
    phoneNumbers.forEach((number) => {
        users.push({ accountId: i.toString(), phoneNumber: number });
    });
}

export let options = {
    scenarios: {
        high_load_test: {
            executor: 'constant-arrival-rate',
            rate: 2000,         
            timeUnit: '1s',
            duration: '1m',     
            preAllocatedVUs: 5, 
        },
    },
};

export default function () {
    let user = users[Math.floor(Math.random() * users.length)];
    const response = http.post("http://localhost:5051/sms/cansendsms", JSON.stringify(user), {
        headers: {
            "Content-Type": "application/json",
        },
    });

    check(response, { "status is 200": (r) => r.status === 200 });
}
