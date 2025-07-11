import { Component, Input, OnInit, OnDestroy, AfterViewChecked, ViewChild, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import * as signalR from '@microsoft/signalr';
import { environment } from '../../environments/environment';

// Define the ChatMessage interface to match your .NET backend
interface ChatMessage {
  sender: string;
  messageContent: string; // Updated property name to match .NET model
  timestamp: Date;
  chatRoomKey?: string;
}

@Component({
  selector: 'app-chat-window',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './chat-window.component.html',
  styleUrls: ['./chat-window.component.scss']
})
export class ChatWindowComponent implements OnInit, OnDestroy, AfterViewChecked {
  @Input() chatRoomKey!: string;
  @Input() chatName!: string;

  messages: ChatMessage[] = [];
  newMessage: string = '';
  public hubConnection!: signalR.HubConnection;
  private chatInitialized: boolean = false;

  @ViewChild('messagesDisplay') private messagesDisplayRef!: ElementRef;

  activeUsersInRoom: number = 0; // New property to display active users

  constructor() { }

  ngOnInit(): void {
    if (!this.chatRoomKey || !this.chatName) {
      console.error('ChatRoomKey or ChatName is missing!');
      return;
    }

    this.initializeSignalR();
  }

  ngOnDestroy(): void {
    if (this.hubConnection) {
      this.hubConnection.stop();
      console.log('SignalR connection stopped.');
    }
  }

  ngAfterViewChecked(): void {
    this.scrollToBottom();
  }

  private scrollToBottom(): void {
    try {
      if (this.messagesDisplayRef?.nativeElement) {
        const element = this.messagesDisplayRef.nativeElement;
        // Use a small timeout to ensure DOM is updated
        setTimeout(() => {
          element.scrollTop = element.scrollHeight;
        }, 0);
      }
    } catch(err) {
      // console.error('Could not scroll to bottom:', err);
    }
  }

  private initializeSignalR(): void {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(`${environment.backendUrl}/chatsHub`, {
        // skipNegotiation: true,
        // transport: signalR.HttpTransportType.WebSockets
         withCredentials: true
      })
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Information)
      .build();

    this.hubConnection.on('ReceiveMessage', (message: ChatMessage) => {
      console.log('Received message:', message);
      message.timestamp = new Date(message.timestamp);
      this.messages.push(message);
    });

    this.hubConnection.on('LoadHistoricalMessages', (historicalMessages: ChatMessage[]) => {
      console.log('Received historical messages:', historicalMessages);
      historicalMessages.forEach(msg => {
        msg.timestamp = new Date(msg.timestamp);
      });
      this.messages = historicalMessages.sort((a, b) => a.timestamp.getTime() - b.timestamp.getTime());
    });

    // Handle user join notifications and update active user count
    this.hubConnection.on('UserJoined', (chatName: string, roomKey: string, activeUsers: number) => {
      if (roomKey === this.chatRoomKey) {
        const joinMessage: ChatMessage = {
          sender: 'System',
          messageContent: `${chatName} has joined the room.`, // Use messageContent
          timestamp: new Date()
        };
        this.messages.push(joinMessage);
        this.activeUsersInRoom = activeUsers; // Update active user count
      }
    });

    // Handle user left notifications (we'll implement this on backend too)
    this.hubConnection.on('UserLeft', (chatName: string, roomKey: string, activeUsers: number) => {
        if (roomKey === this.chatRoomKey) {
            const leftMessage: ChatMessage = {
                sender: 'System',
                messageContent: `${chatName} has left the room.`,
                timestamp: new Date()
            };
            this.messages.push(leftMessage);
            this.activeUsersInRoom = activeUsers;
        }
    });

    // Handle errors from the server
    this.hubConnection.on('Error', (errorMessage: string) => {
      console.error('SignalR Error from server:', errorMessage);
      const errorMsg: ChatMessage = {
        sender: 'System',
        messageContent: `Error: ${errorMessage}`,
        timestamp: new Date()
      };
      this.messages.push(errorMsg);
    });

    // Handle connection errors
    this.hubConnection.onclose((error) => {
      console.error('SignalR connection closed:', error);
    });

    this.hubConnection.onreconnecting((error) => {
      console.warn('SignalR reconnecting:', error);
    });

    this.hubConnection.onreconnected((connectionId) => {
      console.log('SignalR reconnected:', connectionId);
      // Rejoin the room after reconnection
      if (this.chatInitialized) {
        this.hubConnection.invoke('JoinChatRoom', this.chatRoomKey, this.chatName)
          .catch(err => console.error('Error rejoining chat room after reconnection:', err));
      }
    });


    this.hubConnection.start()
      .then(() => {
        console.log('SignalR connection started.');
        if (!this.chatInitialized) {
          // Pass chatName to JoinChatRoom
          this.hubConnection.invoke('JoinChatRoom', this.chatRoomKey, this.chatName)
            .then(() => {
              console.log(`Joined chat room group: ${this.chatRoomKey}`);
              this.chatInitialized = true;
            })
            .catch(err => console.error('Error invoking JoinChatRoom:', err));
        }
      })
      .catch(err => console.error('Error starting SignalR connection:', err));
  }

  sendMessage(): void {
    if (this.newMessage.trim() && this.hubConnection.state === signalR.HubConnectionState.Connected) {
      const messageToSend: ChatMessage = {
        sender: this.chatName,
        messageContent: this.newMessage.trim(),
        timestamp: new Date()
      };

      this.hubConnection.invoke('SendMessage', this.chatRoomKey, messageToSend)
        .then(() => {
          this.newMessage = '';
        })
        .catch(err => console.error('Error invoking SendMessage:', err));
    } else if (this.hubConnection.state !== signalR.HubConnectionState.Connected) {
      console.warn('SignalR connection not established. Cannot send message.');
    }
  }
}