import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { ActivatedRoute, Router } from '@angular/router';
import { environment } from '../environments/environment';
import { ChatWindowComponent } from './chat-window/chat-window.component';

interface CreateRoomRequest {
  roomTitle: string;
  createdBy: string;
  roomPin?: string;
  expiresAt?: Date;
  isPersistent: boolean;
}

interface RoomInfo {
  chatRoomKey: string;
  roomTitle?: string;
  createdBy?: string;
  createdAt: Date;
  expiresAt?: Date;
  requiresPin: boolean;
  isExpired: boolean;
  activeUsers: number;
  lastActivity: Date;
}

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ChatWindowComponent
  ],
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent implements OnInit {
  title = 'Chats96';
  chatName: string = '';
  chatRoomKey: string | null = null;
  isChatActive: boolean = false;
  errorMessage: string | null = null;
  
  // New properties for room creation
  roomTitle: string = '';
  roomPin: string = '';
  expiresAt: string = '';
  isPersistent: boolean = false;
  showCreateRoomForm: boolean = false;
  
  // PIN verification
  showPinDialog: boolean = false;
  enteredPin: string = '';
  roomInfo: RoomInfo | null = null;
  
  // Loading states
  isLoading: boolean = false;
  isCreatingRoom: boolean = false;

  // Signal for theme management
  theme = signal('light-theme');

  private backendUrl = environment.backendUrl + '/api/chat';

  constructor(
    private http: HttpClient,
    private route: ActivatedRoute,
    private router: Router
  ) {}

  ngOnInit(): void {
    const savedTheme = localStorage.getItem('theme');
    if (savedTheme) {
      this.theme.set(savedTheme);
      document.body.classList.add(savedTheme);
    } else {
      document.body.classList.add(this.theme());
    }

    this.route.queryParams.subscribe(params => {
      if (params['key']) {
        this.chatRoomKey = params['key'];
        this.loadRoomInfo(params['key']);
      }
    });
  }

  loadRoomInfo(roomKey: string): void {
    this.isLoading = true;
    this.http.get<RoomInfo>(`${this.backendUrl}/room-info/${roomKey}`)
      .subscribe({
        next: (roomInfo) => {
          this.roomInfo = roomInfo;
          this.isLoading = false;
          
          if (roomInfo.isExpired) {
            this.errorMessage = 'This chat room has expired and is no longer accessible.';
            return;
          }
          
          if (roomInfo.requiresPin) {
            this.showPinDialog = true;
          }
        },
        error: (error) => {
          console.error('Error loading room info:', error);
          this.errorMessage = 'Room not found or an error occurred.';
          this.isLoading = false;
        }
      });
  }

  createNewChat(): void {
    if (!this.chatName.trim()) {
      this.errorMessage = 'Please enter your name.';
      return;
    }
    
    if (!this.roomTitle.trim()) {
      this.errorMessage = 'Please enter a room title.';
      return;
    }
    
    this.errorMessage = null;
    this.isCreatingRoom = true;

    const createRequest: CreateRoomRequest = {
      roomTitle: this.roomTitle.trim(),
      createdBy: this.chatName.trim(),
      roomPin: this.roomPin.trim() || undefined,
      expiresAt: this.expiresAt ? new Date(this.expiresAt) : undefined,
      isPersistent: this.isPersistent || !!this.roomPin.trim() || !!this.expiresAt
    };

    this.http.post<any>(`${this.backendUrl}/create`, createRequest)
      .subscribe({
        next: (response) => {
          this.chatRoomKey = response.key;
          console.log('New chat room created with key:', this.chatRoomKey);
          this.isChatActive = true;
          this.showCreateRoomForm = false;
          this.isCreatingRoom = false;
          this.router.navigate([], { queryParams: { key: this.chatRoomKey }, relativeTo: this.route });
        },
        error: (error) => {
          console.error('Error creating chat room:', error);
          this.errorMessage = 'Failed to create chat room. Please try again.';
          this.isCreatingRoom = false;
        }
      });
  }

  verifyPinAndJoin(): void {
    if (!this.enteredPin.trim()) {
      this.errorMessage = 'Please enter the room PIN.';
      return;
    }
    
    if (!this.chatName.trim()) {
      this.errorMessage = 'Please enter your name.';
      return;
    }

    const verifyRequest = {
      chatRoomKey: this.chatRoomKey,
      userName: this.chatName.trim(),
      roomPin: this.enteredPin.trim()
    };

    this.http.post<any>(`${this.backendUrl}/verify-pin`, verifyRequest)
      .subscribe({
        next: (response) => {
          this.showPinDialog = false;
          this.isChatActive = true;
          this.errorMessage = null;
        },
        error: (error) => {
          console.error('Error verifying PIN:', error);
          this.errorMessage = error.error?.error || 'Invalid PIN. Please try again.';
        }
      });
  }

  joinExistingChat(): void {
    if (!this.chatName.trim()) {
      this.errorMessage = 'Please enter your name.';
      return;
    }
    if (!this.chatRoomKey) {
        this.errorMessage = 'No chat room key provided in URL or input.';
        return;
    }
    
    if (this.roomInfo?.requiresPin) {
      this.showPinDialog = true;
      return;
    }
    
    this.errorMessage = null;
    console.log(`Joining chat room ${this.chatRoomKey} as ${this.chatName}`);
    this.isChatActive = true;
  }

  setChatRoomKeyManually(key: string): void {
      this.chatRoomKey = key;
      this.loadRoomInfo(key);
  }

  toggleTheme(): void {
    const currentTheme = this.theme();
    const newTheme = currentTheme === 'light-theme' ? 'dark-theme' : 'light-theme';

    document.body.classList.remove(currentTheme);
    document.body.classList.add(newTheme);
    localStorage.setItem('theme', newTheme);
    this.theme.set(newTheme);
  }

  shareChatRoom(): void {
    if (this.chatRoomKey) {
      const shareUrl = window.location.origin + this.router.url;
      navigator.clipboard.writeText(shareUrl)
        .then(() => {
          alert('Chat room link copied to clipboard: ' + shareUrl);
        })
        .catch(err => {
          console.error('Could not copy text: ', err);
          alert('Failed to copy link. Please copy manually: ' + shareUrl);
        });
    } else {
      alert('No chat room key available to share.');
    }
  }

  closeCreateForm(): void {
    this.showCreateRoomForm = false;
    this.resetCreateForm();
  }

  closePinDialog(): void {
    this.showPinDialog = false;
    this.enteredPin = '';
    this.errorMessage = null;
  }

  private resetCreateForm(): void {
    this.roomTitle = '';
    this.roomPin = '';
    this.expiresAt = '';
    this.isPersistent = false;
  }

  get currentStatus(): string {
      if (this.isChatActive && this.chatRoomKey && this.chatName) {
          const title = this.roomInfo?.roomTitle || this.chatRoomKey;
          return `You are in "${title}" as "${this.chatName}"`;
      } else if (!this.isChatActive && this.chatRoomKey && this.roomInfo) {
          const title = this.roomInfo.roomTitle || 'Unknown Room';
          return `Ready to join "${title}". ${this.roomInfo.requiresPin ? 'PIN required.' : 'Enter your name and click Join.'}`;
      }
      return 'Create a new chat or join an existing one.';
  }

  get minDateTime(): string {
    return new Date().toISOString().slice(0, 16);
  }
}