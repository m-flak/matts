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
import { Component, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { UserRoleConstants } from '../constants';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { User, UserRegistration, WSAuthEventTypes, WSAuthMessage } from '../models';
import { Observable, Subject, Subscription, delay, first } from 'rxjs';
import { MatExpansionPanel } from '@angular/material/expansion';
import { MonitorService } from '../services/monitor.service';
import { LoadingBarService } from '@ngx-loading-bar/core';
import { ConfigService } from '../services/config.service';
import { HttpParams } from '@angular/common/http';
import { CookieService } from 'ngx-cookie-service';
import { ToastService } from '../services/toast.service';

@Component({
  selector: 'app-login-page',
  templateUrl: './login-page.component.html',
  styleUrls: ['./login-page.component.scss'],
})
export class LoginPageComponent implements OnInit, OnDestroy {
  readonly _roleConstants = UserRoleConstants;
  readonly LOGIN_TYPE_EMPLOYER = 0;
  readonly LOGIN_TYPE_APPLICANT = 1;
  readonly REGISTRATION_TYPE_EMPLOYER = 0;
  readonly REGISTRATION_TYPE_APPLICANT = 1;

  private _subscription: Subscription | null = null;
  private _subscription2: Subscription | null = null;
  private _subscription3: Subscription | null = null;
  private _subscription4: Subscription | null = null;

  loader = this.loadingBar.useRef();
  displayDimmer = false;

  retrievingInformation = false;

  loginFailure = false;
  registrationSuccessful = false;
  registrationTypeMessage = '';
  registrationMode = -1;

  employerLoginForm: FormGroup;
  applicantLoginForm: FormGroup;
  applicantRegistrationForm: FormGroup;
  employerRegistrationForm: FormGroup;

  @ViewChild('employerPanel')
  employerPanel?: MatExpansionPanel;

  @ViewChild('applicantPanel')
  applicantPanel?: MatExpansionPanel;

  @ViewChild('registrationPanel')
  registrationPanel?: MatExpansionPanel;

  constructor(
    private formBuilder: FormBuilder,
    private router: Router,
    private configService: ConfigService,
    private cookieService: CookieService,
    private authService: AuthService,
    private monitorService: MonitorService,
    private toastService: ToastService,
    private loadingBar: LoadingBarService,
  ) {
    this.employerLoginForm = new FormGroup([]);
    this.applicantLoginForm = new FormGroup([]);
    this.applicantRegistrationForm = new FormGroup([]);
    this.employerRegistrationForm = new FormGroup([]);
  }

  ngOnInit(): void {
    this.employerLoginForm = this.formBuilder.group({
      userName: ['', Validators.required],
      password: ['', Validators.required],
    });

    this.applicantLoginForm = this.formBuilder.group({
      userName: ['', Validators.required],
      password: ['', Validators.required],
    });

    this.applicantRegistrationForm = this.formBuilder.group({
      fullName: ['', Validators.required],
      email: ['', [Validators.email, Validators.required]],
      phoneNumber: [
        '',
        [Validators.pattern('^[ s+s/0-9-]*$'), Validators.minLength(10), Validators.maxLength(12), Validators.required],
      ],
      userName: ['', Validators.required],
      password: ['', Validators.required],
    });

    this.employerRegistrationForm = this.formBuilder.group({
      companyName: ['John Doe Corporation', Validators.required], // TODO: Pull from app config
      fullName: ['', Validators.required],
      email: ['', [Validators.email, Validators.required]],
      phoneNumber: [
        '',
        [Validators.pattern('^[ s+s/0-9-]*$'), Validators.minLength(10), Validators.maxLength(12), Validators.required],
      ],
      userName: ['', Validators.required],
      password: ['', Validators.required],
    });

    this._subscription3 = this.loader.value$.subscribe({
      next: _ => {
        this.displayDimmer = true;
      },
      complete: () => {
        this.displayDimmer = false;
      },
    });
  }

  ngOnDestroy(): void {
    if (this._subscription !== null) {
      this._subscription.unsubscribe();
    }
    if (this._subscription2 !== null) {
      this._subscription2.unsubscribe();
    }
    if (this._subscription3 !== null) {
      this._subscription3.unsubscribe();
    }
    if (this._subscription4 !== null) {
      this._subscription4.unsubscribe();
    }
  }

  setRegistrationMode(mode: number): void {
    this.registrationMode = mode;
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
        role: UserRoleConstants.USER_ROLE_EMPLOYER,
      };
      this.displayDimmer = true;
      this._subscription = this.authService
        .loginUser(user)
        .pipe(first())
        .subscribe({
          complete: () => {
            this.monitorService.sendSuccess({ id: 'login' });
            user = null;
            this.loginFailure = false;
            this.router.navigate(['/employer']);
          },
          error: (err: Error) => {
            this.monitorService.sendFailure({ id: 'login' });
            console.error(err?.message);
            this.loginFailure = true;
          },
        });
    } else if (loginType === this.LOGIN_TYPE_APPLICANT) {
      if (this.applicantLoginForm.invalid) {
        return;
      }

      const formData = this.applicantLoginForm.value;
      let user: User | null = {
        userName: formData.userName,
        password: formData.password,
        role: UserRoleConstants.USER_ROLE_APPLICANT,
      };
      this.displayDimmer = true;
      this._subscription = this.authService
        .loginUser(user)
        .pipe(first())
        .subscribe({
          complete: () => {
            this.monitorService.sendSuccess({ id: 'login' });
            user = null;
            this.loginFailure = false;
            this.router.navigate(['/applicant']);
          },
          error: (err: Error) => {
            this.monitorService.sendFailure({ id: 'login' });
            console.error(err?.message);
            this.loginFailure = true;
          },
        });
    }
  }

  performRegistration(registrationType: number): void {
    if (registrationType === this.REGISTRATION_TYPE_EMPLOYER) {
      if (this.employerRegistrationForm.invalid) {
        return;
      }

      const formData = this.employerRegistrationForm.value;
      this.displayDimmer = true;
      this._subscription2 = this.authService
        .registerUser({ ...(formData as UserRegistration), role: UserRoleConstants.USER_ROLE_EMPLOYER })
        .pipe(first())
        .subscribe({
          complete: () => {
            this.monitorService.sendSuccess({ id: 'registration' });
            this.registrationSuccessful = true;
            this.registrationTypeMessage = 'Employer';
            window.scroll({
              top: 0,
              left: 0,
              behavior: 'smooth',
            });
            (this.employerPanel as MatExpansionPanel).expanded = true;
            (this.registrationPanel as MatExpansionPanel).disabled = true;
          },
          error: _ => {
            this.monitorService.sendFailure({ id: 'registration' });
          },
        });
    } else if (registrationType === this.REGISTRATION_TYPE_APPLICANT) {
      if (this.applicantRegistrationForm.invalid) {
        return;
      }

      const formData = this.applicantRegistrationForm.value;
      this.displayDimmer = true;
      this._subscription2 = this.authService
        .registerUser({ ...(formData as UserRegistration), role: UserRoleConstants.USER_ROLE_APPLICANT })
        .pipe(first())
        .subscribe({
          complete: () => {
            this.monitorService.sendSuccess({ id: 'registration' });
            this.registrationSuccessful = true;
            this.registrationTypeMessage = 'Applicant';
            window.scroll({
              top: 0,
              left: 0,
              behavior: 'smooth',
            });
            (this.applicantPanel as MatExpansionPanel).expanded = true;
            (this.registrationPanel as MatExpansionPanel).disabled = true;
          },
          error: _ => {
            this.monitorService.sendFailure({ id: 'registration' });
          },
        });
    }
  }

  clickApplicantLinkedinButton(): void {
    const clientIdentity = this.cookieService.get('XSRF-TOKEN');
    const params = new HttpParams({
      fromObject: {
        response_type: 'code',
        client_id: this.configService.config.linkedinOauth.clientId,
        redirect_uri: this.configService.config.linkedinOauth.redirectUri,
        state: clientIdentity,
        scope: this.configService.config.linkedinOauth.scope,
      },
    });

    this.retrievingInformation = true;
    this.loader.start();
    const linkedinUrl = `https://www.linkedin.com/oauth/v2/authorization?${params.toString()}`;
    let windowPop = window.open(linkedinUrl, '_blank');

    const sendToSocket = new Subject<WSAuthMessage>();
    const socketMessages = this.authService.getWSAuthStream(sendToSocket);
    sendToSocket.next({
      type: WSAuthEventTypes.NONE,
      clientIdentity: '',
      data: null,
    });
    const obs: Observable<UserRegistration> = new Observable(subscriber => {
      let sub: Subscription = socketMessages.pipe(delay(500)).subscribe(jsonMsg => {
        const message: WSAuthMessage = JSON.parse(jsonMsg);
        if (message) {
          if (message.type === WSAuthEventTypes.SERVER_CONNECTION_ESTABLISHED) {
            this.authService.sendWSAuthStart(sendToSocket);
            this.loader.set(25);
          } else if (
            message.type === WSAuthEventTypes.SERVER_OAUTH_STARTED ||
            message.type === WSAuthEventTypes.SERVER_OAUTH_PENDING
          ) {
            const reply: WSAuthMessage = {
              type: WSAuthEventTypes.CLIENT_OAUTH_REQUEST_STATUS,
              clientIdentity: clientIdentity,
              data: null,
            };
            sendToSocket.next(reply);
          } else if (message.type === WSAuthEventTypes.SERVER_OAUTH_ABORTFAIL) {
            this.authService.terminateWSAuthStream(sub, sendToSocket);
            subscriber.error();
            sub.unsubscribe();
          } else if (message.type === WSAuthEventTypes.SERVER_OAUTH_COMPLETED) {
            this.loader.set(50);
            const theData: UserRegistration = message.data;
            subscriber.next(theData);
            this.authService.terminateWSAuthStream(sub, sendToSocket);
            subscriber.complete();
            sub.unsubscribe();
          }
        } else {
          sendToSocket.next({
            type: WSAuthEventTypes.NONE,
            clientIdentity: '',
            data: null,
          });
        }
      });
    });

    this._subscription4 = obs.subscribe({
      next: data => {
        windowPop?.close();
        this.loader.set(75);

        this.applicantRegistrationForm.controls.fullName.setValue(data.fullName);
        this.applicantRegistrationForm.controls.email.setValue(data.email);
        this.applicantRegistrationForm.controls.phoneNumber.setValue(data.phoneNumber);
        
        this.retrievingInformation = false;
      },
      error: _ => {
        this.loader.complete();
        this.toastService.show('There was a problem retrieving your profile information from LinkedIn. Please try again.', {
          classname: 'bg-danger text-light',
          delay: 15000,
          ariaLive: 'assertive',
        });
      },
      complete: () => {
        this.loader.complete();
        this.toastService.show(`Successfully retrieved your profile information from LinkedIn.`, {
          classname: 'bg-success text-light',
          delay: 10000,
        });
      }
    });
  }
}
