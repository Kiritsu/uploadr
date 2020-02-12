import { HttpClient } from '@angular/common/http';
import { BackendMe } from '../data/BackendMe';
import { Inject, Injectable } from '@angular/core';
import { Observable, observable, BehaviorSubject } from 'rxjs';

@Injectable({
    providedIn: 'root'
})
export class BackendApiService {
    subject = new BehaviorSubject<BackendMe>(null);
    
    constructor(public http: HttpClient, @Inject('BASE_URL') public baseUrl: string) { 
        this.getMe();
    }

    getMe(): BehaviorSubject<BackendMe> {
        if (this.subject.getValue() === null) {
            this.http.get<BackendMe>(this.baseUrl + 'api/front/@me').subscribe((result) => {
                this.subject.next(result);
            });
        }

        return this.subject;
    }
}