import { Component, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-nav-menu',
  templateUrl: './nav-menu.component.html'
})
export class NavMenuComponent { 
    public me: NavMenuMe;

    constructor(http: HttpClient, @Inject('BASE_URL') baseUrl: string) {
        http.get<NavMenuMe>(baseUrl + 'api/front/@me').subscribe(result => {
            this.me = result;
        }, error => console.error(error));
    }
}

interface NavMenuMe {
    canSignup: boolean;
    isAuthenticated: boolean;
    isAdmin: boolean;
}