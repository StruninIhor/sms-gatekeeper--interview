import { Component, OnInit, OnDestroy } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { interval, Subscription } from 'rxjs';
import * as signalR from '@microsoft/signalr';

interface MetricPoint {
  timestamp: string;
  messagesPerSecond: number;
}

@Component({
  selector: 'app-realtime-dashboard',
  templateUrl: './realtime-dashboard.component.html',
  standalone: true,
  imports: [CommonModule]
})
export class RealtimeDashboardComponent implements OnInit, OnDestroy {
  private updateSubscription: Subscription | undefined;
  private hubConnection: signalR.HubConnection | undefined;
  private readonly API_URL = 'http://localhost:3000';

  currentRate = 0;
  connectionError = false;

  constructor(private http: HttpClient) {}

  ngOnInit(): void {
    this.setupSignalRConnection();
  }

  ngOnDestroy(): void {
    this.updateSubscription?.unsubscribe();
    this.hubConnection?.stop();
  }

  private setupSignalRConnection(): void {
    try {
      this.hubConnection = new signalR.HubConnectionBuilder()
        .withUrl(`${this.API_URL}/metrics`)
        .withAutomaticReconnect()
        .build();

      this.hubConnection.start()
        .then(() => {
          console.log('SignalR Connected');
          this.connectionError = false;

          this.hubConnection!.on('MetricsUpdate', (metric: MetricPoint) => {
            this.updateMetrics(metric);
          });
        })
        .catch((err: Error) => {
          console.error('SignalR Connection Error:', err);
          this.connectionError = true;
          this.fallbackToPolling();
        });
    } catch (err) {
      console.error('SignalR Setup Error:', err);
      this.connectionError = true;
      this.fallbackToPolling();
    }
  }

  private fallbackToPolling(): void {
    // If SignalR fails, fall back to polling
    this.updateSubscription = interval(1000).subscribe(() => {
      this.http.get<MetricPoint>(`${this.API_URL}/api/metrics/current`).subscribe(
        metric => this.updateMetrics(metric)
      );
    });
  }

  private updateMetrics(metric: MetricPoint): void {
    this.currentRate = metric.messagesPerSecond;
  }
}
