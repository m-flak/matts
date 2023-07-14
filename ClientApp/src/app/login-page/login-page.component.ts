import { Component, OnInit } from '@angular/core';
import { UserRoleConstants } from '../constants';

@Component({
  selector: 'app-login-page',
  templateUrl: './login-page.component.html',
  styleUrls: ['./login-page.component.scss']
})
export class LoginPageComponent implements OnInit {
  readonly _roleConstants = UserRoleConstants;

  constructor() { }

  ngOnInit(): void {
  }


  fakeLoginEmp(): void {
    sessionStorage.setItem('authducktape', 'true');
  }

  fakeLoginApp(): void {
    sessionStorage.setItem('authducktape', 'true');
  }
}
