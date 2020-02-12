import { Component, OnInit } from '@angular/core';
import { BackendMe } from '../data/BackendMe';
import { BackendApiService } from '../services/BackendApiService';

@Component({
  selector: 'signup',
  templateUrl: './signup.component.html'
})
export class SignupComponent implements OnInit { 
    private backend: BackendApiService;
    public me: BackendMe;

    constructor(backend: BackendApiService) {
        this.backend = backend;
    }

    ngOnInit() {
        this.backend.getMe().subscribe(x => this.me = x);
    }
}