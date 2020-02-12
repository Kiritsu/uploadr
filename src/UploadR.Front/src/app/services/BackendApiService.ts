import { HttpClient } from '@angular/common/http';
import { BackendMe } from '../data/BackendMe';
import { Inject, Injectable } from '@angular/core';
import { Observable, observable } from 'rxjs';

@Injectable({
    providedIn: 'root'
})
export class BackendApiService {
    http: HttpClient;
    baseUrl: String;

    meObservable: Observable<BackendMe>;
    
    constructor(http: HttpClient, @Inject('BASE_URL') baseUrl: string) {
        this.http = http;
        this.baseUrl = baseUrl;
    }

    getMe(): Observable<BackendMe> {
        return this.meObservable 
            ? this.meObservable 
            : this.meObservable = new Observable<BackendMe>(x => {
                this.http.get<BackendMe>(this.baseUrl + 'api/front/@me').subscribe((result) => {
                    x.next(result);
                    x.complete();
                });
            });
    }
}