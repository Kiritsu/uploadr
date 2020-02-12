import { Component, OnInit } from '@angular/core';
import { BackendMe } from '../data/BackendMe';
import { BackendApiService } from '../services/backend.api.service';

@Component({
  selector: 'signup',
  templateUrl: './signup.component.html'
})
export class SignupComponent implements OnInit {
    public me: BackendMe;

    constructor(public backend: BackendApiService) { }

    ngOnInit() {
        this.backend.getMe().subscribe({
            next: me => this.me = me
        });
    }
}