import { Component, OnInit } from '@angular/core';
import { BackendMe } from '../data/backendme.model';
import { BackendApiService } from '../services/backend.api.service';

@Component({
  selector: 'app-nav-menu',
  templateUrl: './nav-menu.component.html'
})
export class NavMenuComponent implements OnInit { 
    public me: BackendMe;

    constructor(public backend: BackendApiService) { }

    ngOnInit() {
        this.backend.getMe().subscribe({
            next: me => this.me = me
        });
    }
}