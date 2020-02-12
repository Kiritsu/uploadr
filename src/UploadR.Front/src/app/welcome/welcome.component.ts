import { Component, OnInit } from '@angular/core';
import { BackendApiService } from '../services/backend.api.service';
import { BackendMe } from '../data/backendme.model';

@Component({
    selector: 'welcome',
    templateUrl: './welcome.component.html'
})
export class WelcomeComponent {
    title = 'UploadR';

    public me: BackendMe;

    constructor(public backend: BackendApiService) { }

    ngOnInit() {
        this.backend.getMe().subscribe({
          next: me => this.me = me
        });
    }
}
