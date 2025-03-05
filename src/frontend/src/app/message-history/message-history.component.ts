import { Component, OnInit, ViewChild } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { HistoryRecord } from '../interfaces/history-record';
import { CommonModule } from '@angular/common';
import { FormsModule, NgForm } from '@angular/forms';
import { catchError, of } from 'rxjs';

interface SearchCriteria {
  phoneNumber?: string;
  accountId?: string;
  startDate?: string;
  endDate?: string;
}

interface HistoryResponse {
  records: HistoryRecord[];
  totalRecords: number;
  totalPages: number;
}

interface PaginationSettings {
  pageSize: number;
  currentPage: number;
}

@Component({
  selector: 'app-message-history',
  templateUrl: './message-history.component.html',
  styleUrls: ['./message-history.component.css'],
  standalone: true,
  imports: [CommonModule, FormsModule]
})
export class MessageHistoryComponent implements OnInit {
  private readonly API_URL = 'http://localhost:5164';
  private readonly FALLBACK_URL = 'http://localhost:3000';
  private readonly STORAGE_KEY = 'messageHistoryPagination';

  @ViewChild('searchForm') searchForm!: NgForm;
  @ViewChild('phoneNumberInput') phoneNumberInput: any;

  historyRecords: HistoryRecord[] = [];
  currentPage: number;
  pageSize: number;
  totalPages = 1;
  totalRecords = 0;
  searchCriteria: SearchCriteria = {};
  usingFallback = false;

  constructor(private http: HttpClient) {
    const savedSettings = this.loadPaginationSettings();
    this.currentPage = savedSettings.currentPage;
    this.pageSize = savedSettings.pageSize;
  }

  ngOnInit(): void {
    this.loadHistoryRecords();
  }

  private loadPaginationSettings(): PaginationSettings {
    const savedSettings = localStorage.getItem(this.STORAGE_KEY);
    if (savedSettings) {
      return JSON.parse(savedSettings);
    }
    return { pageSize: 10, currentPage: 1 };
  }

  private savePaginationSettings(): void {
    const settings: PaginationSettings = {
      pageSize: this.pageSize,
      currentPage: this.currentPage
    };
    localStorage.setItem(this.STORAGE_KEY, JSON.stringify(settings));
  }

  getReasonClass(record: HistoryRecord): string {
    if (record.allowed) {
      return 'badge bg-success';
    }
    return record.accountRateHit ? 'badge bg-danger' : 'badge bg-warning';
  }

  getReasonText(record: HistoryRecord): string {
    if (record.allowed) {
      return '-';
    }
    return record.accountRateHit ? 'Account Limit' : 'Number Limit';
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

    this.http.get<HistoryResponse>(`${this.API_URL}/api/history?${params.toString()}`).pipe(
      catchError((error: HttpErrorResponse) => {
        console.error('Main API Error:', error);
        this.usingFallback = true;
        return this.http.get<HistoryResponse>(`${this.FALLBACK_URL}/api/history?${params.toString()}`);
      })
    ).subscribe(response => {
      this.historyRecords = response.records;
      this.totalRecords = response.totalRecords;
      this.totalPages = response.totalPages;
    });
  }

  onPageChange(page: number): void {
    if (page >= 1 && page <= this.totalPages) {
      this.currentPage = page;
      this.savePaginationSettings();
      this.loadHistoryRecords();
    }
  }

  onPageSizeChange(): void {
    this.currentPage = 1;
    this.savePaginationSettings();
    this.loadHistoryRecords();
  }

  onSearch(): void {
    if (this.searchForm.valid || !this.searchCriteria.phoneNumber) {
      this.currentPage = 1;
      this.savePaginationSettings();
      this.loadHistoryRecords();
    }
  }

  resetSearch(): void {
    this.searchCriteria = {};
    this.currentPage = 1;
    this.savePaginationSettings();
    if (this.searchForm) {
      this.searchForm.resetForm();
    }
    this.loadHistoryRecords();
  }
}
