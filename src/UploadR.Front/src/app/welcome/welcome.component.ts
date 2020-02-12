import { Component, OnInit } from '@angular/core';
import { BackendApiService } from '../services/BackendApiService';
import { BackendMe } from '../data/BackendMe';

@Component({
  selector: 'welcome',
  templateUrl: './welcome.component.html'
})
export class WelcomeComponent {
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
