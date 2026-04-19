import { Injectable, OnDestroy } from '@angular/core';
import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { Subject } from 'rxjs';

export interface AuctionTimeUpdate {
  productId: string;
  timeRemainingSeconds: number;
  isActive: boolean;
  hasEnded: boolean;
  status: string;
}

@Injectable({ providedIn: 'root' })
export class AuctionSignalService implements OnDestroy {
  private hubConnection: HubConnection | null = null;
  readonly updates$ = new Subject<AuctionTimeUpdate[]>();

  start(): void {
    if (this.hubConnection) return;

    this.hubConnection = new HubConnectionBuilder()
      .withUrl('/hubs/auction', {
        withCredentials: true
      })
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Information)
      .build();

    this.hubConnection.on('AuctionTimeUpdate', (updates: AuctionTimeUpdate[] | AuctionTimeUpdate) => {
      const arr = Array.isArray(updates) ? updates : [updates];
      this.updates$.next(arr);
    });

    this.hubConnection.start()
      .then()
      .catch(err => console.error('SignalR error:', err));
  }
    
  stop(): void {
    this.hubConnection?.stop();
    this.hubConnection = null;
  }

  ngOnDestroy(): void {
    this.stop();
  }
}
