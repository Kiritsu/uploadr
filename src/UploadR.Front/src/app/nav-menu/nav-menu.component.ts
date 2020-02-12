import { Component, Inject, OnInit } from '@angular/core';
import { BackendMe } from '../data/BackendMe';
import { BackendApiService } from '../services/BackendApiService';

@Component({
  selector: 'app-nav-menu',
  templateUrl: './nav-menu.component.html'
})
export class NavMenuComponent implements OnInit { 
    private backend: BackendApiService;
    public me: BackendMe;

    constructor(backend: BackendApiService) {
        this.backend = backend;
    }

    ngOnInit() {
        this.backend.getMe().subscribe(x => this.me = x);
    }
}