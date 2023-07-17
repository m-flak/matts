import { Component, OnInit } from '@angular/core';
import { UserRoleConstants } from '../constants';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { Router } from '@angular/router';

@Component({
  selector: 'app-login-page',
  templateUrl: './login-page.component.html',
  styleUrls: ['./login-page.component.scss']
})
export class LoginPageComponent implements OnInit {
  readonly _roleConstants = UserRoleConstants;
  readonly LOGIN_TYPE_EMPLOYER = 0;
  readonly LOGIN_TYPE_APPLICANT = 1;

  employerLoginForm: FormGroup;
  applicantLoginForm: FormGroup;

  constructor(private formBuilder: FormBuilder, private router: Router) { 
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

  performLogin(loginType: number): void {
    if (loginType === this.LOGIN_TYPE_EMPLOYER) {
      if (this.employerLoginForm.invalid) {
        return;
      }

      // TODO: Implement login
      const formData = this.employerLoginForm.value;
      console.log(`${formData.userName}:${formData.password}`);
      sessionStorage.setItem('authducktape', 'true');
      this.router.navigate(['/employer']);
    }
    else if (loginType === this.LOGIN_TYPE_APPLICANT) {
      if (this.applicantLoginForm.invalid) {
        return;
      }

      // TODO: Implement login
      const formData = this.applicantLoginForm.value;
      console.log(`${formData.userName}:${formData.password}`);
      sessionStorage.setItem('authducktape', 'true');
      this.router.navigate(['/applicant']);
    }
  }
}
