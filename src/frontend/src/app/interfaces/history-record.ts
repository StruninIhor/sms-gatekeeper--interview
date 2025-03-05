export interface HistoryRecord {
  id: number;
  requestedAt: Date;
  phoneNumber: string;
  accountId: string;
  allowed: boolean;
  accountRateHit: boolean;
}
