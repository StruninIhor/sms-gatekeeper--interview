import { Routes } from '@angular/router';
import { MessageHistoryComponent } from './message-history/message-history.component';
import { RealtimeDashboardComponent } from './realtime-dashboard/realtime-dashboard.component';

export const routes: Routes = [
  { path: '', redirectTo: 'realtime', pathMatch: 'full' },
  { path: 'realtime', component: RealtimeDashboardComponent },
  { path: 'history', component: MessageHistoryComponent }
];
