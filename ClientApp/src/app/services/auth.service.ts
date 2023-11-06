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

import { Location } from '@angular/common';
import { HttpClient, HttpErrorResponse, HttpHeaders, HttpParams, HttpResponse } from '@angular/common/http';
import { Injectable, Inject } from '@angular/core';
import { JwtHelperService } from '@auth0/angular-jwt';
import { User, UserRegistration, WSAuthEventTypes, WSAuthMessage } from '../models';
import { Observable, Subject, Subscription, catchError, map, of, share, switchMap, tap, throwError } from 'rxjs';
import { CookieService } from 'ngx-cookie-service';
import makeWebSocketObservable, { GetWebSocketResponses } from 'rxjs-websockets';

export interface CurrentUser extends User {
  applicantId: string | null;
  employerId: string | null;
}

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private _currentUser: CurrentUser | null = null;

  private _oauthSocket$: Observable<GetWebSocketResponses<any>> | null = null;

  constructor(
    private http: HttpClient,
    private jwtHelper: JwtHelperService,
    private cookieService: CookieService,
    @Inject('BASE_URL') private baseUrl: string,
    @Inject('WS_BASE_URL') private wsBaseUrl: string,
  ) {}

  get currentUser(): CurrentUser | null {
    return this._currentUser;
  }
  set currentUser(user: CurrentUser | null) {
    this._currentUser = user;
  }

  private _populateCurrentUser() {
    const decodedToken: any = this.jwtHelper.decodeToken();
    let name: string | null = null;
    let role: string | null = null;
    let applicantId: string | null = null;
    let employerId: string | null = null;

    if (decodedToken !== null && decodedToken.hasOwnProperty('role')) {
      role = decodedToken.role as string;
    }
    if (decodedToken !== null && decodedToken.hasOwnProperty('sub')) {
      name = decodedToken.sub as string;
    }
    if (decodedToken !== null && decodedToken.hasOwnProperty('applicantId')) {
      applicantId = decodedToken.applicantId as string;
    }
    if (decodedToken !== null && decodedToken.hasOwnProperty('employerId')) {
      employerId = decodedToken.employerId as string;
    }

    if (name !== null && role !== null) {
      this.currentUser = {
        userName: name,
        password: '',
        role: role,
        applicantId: applicantId,
        employerId: employerId,
      };
    }
  }

  hasToken(): boolean {
    return localStorage.getItem('access_token') !== null;
  }

  isLoggedIn(): boolean {
    return this.currentUser !== null && this.hasToken();
  }

  loginUser(user?: User): Observable<string> {
    const currentToken: string | null = localStorage.getItem('access_token');
    if (currentToken !== null) {
      return of(currentToken).pipe(tap(token => this._populateCurrentUser()));
    }

    const endpoint = '/api/v1/auth/login';
    return this.http.post(Location.joinWithSlash(this.baseUrl, endpoint), user, { responseType: 'text' }).pipe(
      catchError((e: HttpErrorResponse) => throwError(() => new Error(e?.error))),
      tap(token => localStorage.setItem('access_token', token)),
      tap(token => this._populateCurrentUser()),
    );
  }

  registerUser(newUser: UserRegistration): Observable<HttpResponse<any>> {
    const endpoint = `/api/v1/auth/register`;

    const httpHeaders = new HttpHeaders({
      'Content-Type': 'application/json',
    });

    return this.http
      .post(Location.joinWithSlash(this.baseUrl, endpoint), newUser, { headers: httpHeaders, observe: 'response' })
      .pipe(catchError((e: HttpErrorResponse) => throwError(() => new Error(e?.error))));
  }

  logoutUser() {
    localStorage.removeItem('access_token');
    this.currentUser = null;
  }

  getLoggedInUserRole(): string | null {
    let role: string | null = null;

    if (this.currentUser !== null) {
      role = this.currentUser.role;
    }

    return role;
  }

  terminateWSAuthStream(connection: Subscription, sendToSocket: Subject<WSAuthMessage>) {
    sendToSocket.complete();
    connection.unsubscribe();
    this._oauthSocket$ = null;
  }

  sendWSAuthStart(sendToSocket: Subject<WSAuthMessage>) {
    sendToSocket.next({
      type: WSAuthEventTypes.CLIENT_OAUTH_START,
      clientIdentity: this.cookieService.get('XSRF-TOKEN'),
      data: null,
    });
  }

  getWSAuthStream(sendToSocket: Subject<WSAuthMessage>) {
    this._oauthSocket$ =
      this._oauthSocket$ !== null
        ? this._oauthSocket$
        : makeWebSocketObservable(Location.joinWithSlash(this.wsBaseUrl, '/api/v1/ws/oauth/linkedin'));

    return this._oauthSocket$.pipe(
      switchMap((getResponses: GetWebSocketResponses) => {
        return getResponses(sendToSocket.pipe(map(m => JSON.stringify(m)))) as unknown as string;
      }),
      share(),
      map((message: string) => message),
      catchError(e => {
        console.error('OAuth WS error: ', e);
        return [];
      }),
    );
  }
}
