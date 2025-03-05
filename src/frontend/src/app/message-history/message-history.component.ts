import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { HistoryRecord } from '../interfaces/history-record';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

interface SearchCriteria {
  phoneNumber?: string;
  accountId?: string;
  startDate?: string;
  endDate?: string;
}

@Component({
  selector: 'app-message-history',
  templateUrl: './message-history.component.html',
  styleUrls: ['./message-history.component.css'],
  standalone: true,
  imports: [CommonModule, FormsModule]
})
export class MessageHistoryComponent implements OnInit {
  historyRecords: HistoryRecord[] = [];
  currentPage = 1;
  pageSize = 10;
  totalPages = 1;
  totalRecords = 0;
  searchCriteria: SearchCriteria = {};

  constructor(private http: HttpClient) {}

  ngOnInit(): void {
    this.loadHistoryRecords();
  }

  loadHistoryRecords(): void {
    const params = new URLSearchParams({
      page: this.currentPage.toString(),
      pageSize: this.pageSize.toString(),
      ...(this.searchCriteria.phoneNumber && { phoneNumber: this.searchCriteria.phoneNumber }),
      ...(this.searchCriteria.accountId && { accountId: this.searchCriteria.accountId }),
      ...(this.searchCriteria.startDate && { startDate: this.searchCriteria.startDate }),
      ...(this.searchCriteria.endDate && { endDate: this.searchCriteria.endDate })
    });

    const url = `http://localhost:3000/api/history?${params.toString()}`;

    this.http.get<{
      records: HistoryRecord[],
      totalRecords: number,
      totalPages: number
    }>(url).subscribe(response => {
      this.historyRecords = response.records;
      this.totalRecords = response.totalRecords;
      this.totalPages = response.totalPages;
    });
  }

  onPageChange(page: number): void {
    if (page >= 1 && page <= this.totalPages) {
      this.currentPage = page;
      this.loadHistoryRecords();
    }
  }

  onPageSizeChange(): void {
    this.currentPage = 1; // Reset to first page when changing page size
    this.loadHistoryRecords();
  }

  onSearch(): void {
    this.currentPage = 1; // Reset to first page when searching
    this.loadHistoryRecords();
  }

  resetSearch(): void {
    this.searchCriteria = {};
    this.currentPage = 1;
    this.loadHistoryRecords();
  }
}
