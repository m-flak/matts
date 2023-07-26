/* matts
 * "Matthew's ATS" - Portfolio Project
 * Copyright (C) 2023  Matthew E. Kehrer <matthew@kehrer.dev>
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 * 
 * You should have received a copy of the GNU Affero General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
**/
import { Component, OnDestroy, OnInit } from '@angular/core';
import { UserRoleConstants } from '../constants';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { User } from '../models';
import { Subscription, first } from 'rxjs';

@Component({
  selector: 'app-login-page',
  templateUrl: './login-page.component.html',
  styleUrls: ['./login-page.component.scss']
})
export class LoginPageComponent implements OnInit, OnDestroy {
  readonly _roleConstants = UserRoleConstants;
  readonly LOGIN_TYPE_EMPLOYER = 0;
  readonly LOGIN_TYPE_APPLICANT = 1;
  readonly REGISTRATION_TYPE_EMPLOYER = 0;
  readonly REGISTRATION_TYPE_APPLICANT = 1;

  private _subscription: Subscription | null = null;

  loginFailure = false;
  registrationSuccessful = false;
  registrationTypeMessage = '';

  employerLoginForm: FormGroup;
  applicantLoginForm: FormGroup;
  applicantRegistrationForm: FormGroup;

  constructor(private formBuilder: FormBuilder, private router: Router, private authService: AuthService) { 
    this.employerLoginForm = new FormGroup([]);
    this.applicantLoginForm = new FormGroup([]);
    this.applicantRegistrationForm = new FormGroup([]);
  }

  ngOnInit(): void {
    this.employerLoginForm = this.formBuilder.group({
      userName: ['', Validators.required],
      password: ['', Validators.required]
    });

    this.applicantLoginForm = this.formBuilder.group({
      userName: ['', Validators.required],
      password: ['', Validators.required]
    });

    this.applicantRegistrationForm = this.formBuilder.group({
      fullName: ['', Validators.required], 
      userName: ['', Validators.required],
      password: ['', Validators.required]
    });
  }

  ngOnDestroy(): void {
    if (this._subscription !== null) {
      this._subscription.unsubscribe();
    }
  }

  performLogin(loginType: number): void {
    if (loginType === this.LOGIN_TYPE_EMPLOYER) {
      if (this.employerLoginForm.invalid) {
        return;
      }

      const formData = this.employerLoginForm.value;
      let user: User | null = {
        userName: formData.userName,
        password: formData.password,
        role: UserRoleConstants.USER_ROLE_EMPLOYER
      };
      this._subscription = this.authService.loginUser(user).pipe(first()).subscribe({
        complete: () => {
          user = null;
          this.loginFailure = false;
          this.router.navigate(['/employer']);
        },
        error: err => {
          console.error(err);
          this.loginFailure = true;
        }});
    }
    else if (loginType === this.LOGIN_TYPE_APPLICANT) {
      if (this.applicantLoginForm.invalid) {
        return;
      }

      const formData = this.applicantLoginForm.value;
      let user: User | null = {
        userName: formData.userName,
        password: formData.password,
        role: UserRoleConstants.USER_ROLE_APPLICANT
      };
      this._subscription = this.authService.loginUser(user).pipe(first()).subscribe({
        complete: () => {
          user = null;
          this.loginFailure = false;
          this.router.navigate(['/applicant']);
        },
        error: err => {
          console.error(err);
          this.loginFailure = true;
        }});
    }
  }

  performRegistration(registrationType: number): void {
    if (registrationType === this.REGISTRATION_TYPE_EMPLOYER) {
      return;
    }
    else if (registrationType === this.REGISTRATION_TYPE_APPLICANT) {
      if (this.applicantRegistrationForm.invalid) {
        return;
      }

      const formData = this.applicantRegistrationForm.value;
      console.log(formData);
      this.registrationSuccessful = true;
      this.registrationTypeMessage = 'Applicant';
    }
  }
}
