import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { ActivatedRoute, Router } from '@angular/router';
import { environment } from '../environments/environment';
import { ChatWindowComponent } from './chat-window/chat-window.component'; // Import ChatWindowComponent

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ChatWindowComponent // Add ChatWindowComponent to imports
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

  // Signal for theme management (Angular 16+)
  theme = signal('light-theme'); // Default theme

  private backendUrl = environment.backendUrl + '/api/chat';

  constructor(
    private http: HttpClient,
    private route: ActivatedRoute,
    private router: Router
  ) {}

  ngOnInit(): void {
    // Apply initial theme from localStorage or default
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
        this.isChatActive = false; // User needs to enter a chat name to fully join
        console.log('Detected chat room key in URL:', this.chatRoomKey);
      }
    });
  }

  createNewChat(): void {
    if (!this.chatName.trim()) {
      this.errorMessage = 'Please enter a chat name.';
      return;
    }
    this.errorMessage = null;

    this.http.post<{ key: string }>(`${this.backendUrl}/create`, {})
      .subscribe({
        next: (response) => {
          this.chatRoomKey = response.key;
          console.log('New chat room created with key:', this.chatRoomKey);
          this.isChatActive = true;
          this.router.navigate([], { queryParams: { key: this.chatRoomKey }, relativeTo: this.route });
        },
        error: (error) => {
          console.error('Error creating chat room:', error);
          this.errorMessage = 'Failed to create chat room. Please try again.';
        }
      });
  }

  joinExistingChat(): void {
    if (!this.chatName.trim()) {
      this.errorMessage = 'Please enter a chat name.';
      return;
    }
    if (!this.chatRoomKey) {
        this.errorMessage = 'No chat room key provided in URL or input.';
        return;
    }
    this.errorMessage = null;
    console.log(`Joining chat room ${this.chatRoomKey} as ${this.chatName}`);
    this.isChatActive = true;
  }

  setChatRoomKeyManually(key: string): void {
      this.chatRoomKey = key;
  }

  toggleTheme(): void {
    const currentTheme = this.theme();
    const newTheme = currentTheme === 'light-theme' ? 'dark-theme' : 'light-theme';

    document.body.classList.remove(currentTheme);
    document.body.classList.add(newTheme);
    localStorage.setItem('theme', newTheme);
    this.theme.set(newTheme);
  }

  // --- Share Button Logic ---
  shareChatRoom(): void {
    if (this.chatRoomKey) {
      const shareUrl = window.location.origin + this.router.url; // Get current URL with query params
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

  get currentStatus(): string {
      if (this.isChatActive && this.chatRoomKey && this.chatName) {
          return `You are in chat room "${this.chatRoomKey}" as "${this.chatName}"`;
      } else if (!this.isChatActive && this.chatRoomKey) {
          return `You are about to join room "${this.chatRoomKey}". Please enter your chat name and click Join.`;
      }
      return 'Create a new chat or join an existing one.';
  }
}