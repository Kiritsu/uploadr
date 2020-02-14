import { HttpClient } from '@angular/common/http';
import { BackendMe } from '../data/backendme.model';
import { Inject, Injectable } from '@angular/core';
import { Observable, observable, BehaviorSubject } from 'rxjs';

@Injectable({
    providedIn: 'root'
})
export class BackendApiService {
    backendMeSubject = new BehaviorSubject<BackendMe>(null);
    
    constructor(public http: HttpClient, @Inject('BASE_URL') public baseUrl: string) { 
        this.getMe();
    }

    getMe(): BehaviorSubject<BackendMe> {
        if (this.backendMeSubject.getValue() === null) {
            this.http.get<BackendMe>(this.baseUrl + 'api/front/@me').subscribe((result) => {
                this.backendMeSubject.next(result);
            });
        }

        return this.backendMeSubject;
    }

    signup(email: String): Observable<any> {
        return this.http.post(this.baseUrl + 'api/user', { email });
    }
}