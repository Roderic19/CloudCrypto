import { Component, inject, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { environment } from '../environments/environment';
import { HttpClient } from '@angular/common/http';

interface Response {
  message: string;
}

@Component({
  selector: 'app-root',
  imports: [RouterOutlet],
  templateUrl: './app.html',
  styleUrl: './app.scss',
})
export class App {
  protected readonly title = signal('cloud-crypto');
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/api/test/ollama`;

  protected response = signal<string | null>(null);
  protected error = signal<string | null>(null);

  send(){
    this.response.set(null);
    this.error.set(null);

    this.http.get<Response>(this.apiUrl).subscribe({
      next: data => {
        this.response.set(data.message);
      },
      error: error => {
        this.error.set('Failed to reach Ollama: ' + error.message);
      }
    });
  }
}
