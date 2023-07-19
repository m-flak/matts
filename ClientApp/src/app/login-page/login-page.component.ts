import { Component, OnDestroy, OnInit } from '@angular/core';
import { UserRoleConstants } from '../constants';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { User } from '../models';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-login-page',
  templateUrl: './login-page.component.html',
  styleUrls: ['./login-page.component.scss']
})
export class LoginPageComponent implements OnInit, OnDestroy {
  readonly _roleConstants = UserRoleConstants;
  readonly LOGIN_TYPE_EMPLOYER = 0;
  readonly LOGIN_TYPE_APPLICANT = 1;

  private _subscription: Subscription | null = null;

  loginFailure = false;

  employerLoginForm: FormGroup;
  applicantLoginForm: FormGroup;

  constructor(private formBuilder: FormBuilder, private router: Router, private authService: AuthService) { 
    this.employerLoginForm = new FormGroup([]);
    this.applicantLoginForm = new FormGroup([]);
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
        password: formData.password
      };
      this._subscription = this.authService.loginUser(user, UserRoleConstants.USER_ROLE_EMPLOYER).subscribe(() => {
        user = null;
        this.router.navigate(['/employer']);
      },
      () => {
        this.loginFailure = true;
      });
    }
    else if (loginType === this.LOGIN_TYPE_APPLICANT) {
      if (this.applicantLoginForm.invalid) {
        return;
      }

      const formData = this.applicantLoginForm.value;
      let user: User | null = {
        userName: formData.userName,
        password: formData.password
      };
      this._subscription = this.authService.loginUser(user, UserRoleConstants.USER_ROLE_APPLICANT).subscribe(() => {
        user = null;
        this.router.navigate(['/applicant']);
      },
      () => {
        this.loginFailure = true;
      });
    }
  }
}
