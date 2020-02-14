import { Component, OnInit } from '@angular/core';
import { BackendMe } from '../data/backendme.model';
import { BackendApiService } from '../services/backend.api.service';
import { NgForm, FormControl, FormGroup, EmailValidator, FormBuilder, Validators } from '@angular/forms';

@Component({
  selector: 'signup',
  templateUrl: './signup.component.html'
})
export class SignupComponent implements OnInit {
    public me: BackendMe;
    public signupForm: FormGroup;

    public invalid: boolean;
    public alreadyInUse: boolean;
    public signupOk: boolean;
    public routeDisabled: boolean;

    public token: string;

    constructor(public backend: BackendApiService, private formBuilder: FormBuilder) {
        this.signupForm = this.formBuilder.group({
            email: ['', [ Validators.required, Validators.email ]]
        });
    }

    ngOnInit() {
        this.backend.getMe().subscribe({
            next: me => this.me = me
        });
    }

    signup() {
        this.routeDisabled = false;
        this.invalid = false;
        this.signupOk = false;
        this.alreadyInUse = false;

        if (this.signupForm.invalid) {
            this.invalid = true;

            return;
        }

        this.backend.signup(this.signupForm.value.email).subscribe(x => {
            this.token = x.token;
            this.signupOk = true;
        }, y => {
            switch (y.error.code) {
                case 31: this.invalid = true; break;
                case 30: this.alreadyInUse = true; break;
                case 4: this.routeDisabled = true; break;
            }
        });
    }
}