import { Component, OnInit } from '@angular/core';
import { BackendApiService } from './services/BackendApiService';
import { BackendMe } from './data/BackendMe';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html'
})
export class AppComponent implements OnInit {
  title = 'UploadR';

  private backend: BackendApiService;
  public me: BackendMe;

  constructor(backend: BackendApiService) {
      this.backend = backend;
  }

  ngOnInit() {
      this.backend.getMe().subscribe(x => this.me = x);
  }
}
