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

import { Location } from "@angular/common";
import { HttpClient, HttpHeaders, HttpParams, HttpResponse } from "@angular/common/http";
import { Injectable, Inject } from "@angular/core";
import { JwtHelperService } from "@auth0/angular-jwt";
import { User, UserRegistration } from "../models";
import { Observable, catchError, of, tap, throwError } from "rxjs";

export interface CurrentUser extends User {
    applicantId: string | null;
};

@Injectable({
    providedIn: 'root'
})
export class AuthService {
    private _currentUser: CurrentUser | null = null;

    constructor(
        private http: HttpClient,
        private jwtHelper: JwtHelperService,
        @Inject('BASE_URL') private baseUrl: string
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

        if (decodedToken !== null && decodedToken.hasOwnProperty('role')) {
            role = (decodedToken.role as string);
        }
        if (decodedToken !== null && decodedToken.hasOwnProperty('sub')) {
            name = (decodedToken.sub as string);
        }
        if (decodedToken !== null && decodedToken.hasOwnProperty('applicantId')) {
            applicantId = (decodedToken.applicantId as string);
        }

        if (name !== null && role !== null) {
            this.currentUser = {
                userName: name,
                password: '',
                role: role,
                applicantId: applicantId
            };
        }
    }

    hasToken(): boolean {
        return ( localStorage.getItem('access_token') !== null );
    }

    isLoggedIn(): boolean {
        return ( this.currentUser !== null && this.hasToken() );
    }

    loginUser(user?: User) : Observable<string> {
        const currentToken: string | null = localStorage.getItem('access_token');
        if (currentToken !== null) {
            return of(currentToken).pipe(
                tap(token => this._populateCurrentUser())
            );
        }

        const endpoint = '/auth/login';
        return this.http.post(Location.joinWithSlash(this.baseUrl, endpoint), user, { responseType: 'text' })
            .pipe(
                catchError(e => throwError(() => new Error(e))),
                tap(token => localStorage.setItem('access_token', token)),
                tap(token => this._populateCurrentUser())
            );
    }

    registerUser(newUser: UserRegistration): Observable<HttpResponse<any>> {
        const endpoint = `/auth/register`;

        const httpHeaders = new HttpHeaders({
            'Content-Type' : 'application/json'
        });

        return this.http.post(Location.joinWithSlash(this.baseUrl, endpoint), newUser, { headers: httpHeaders, observe: "response" })
            .pipe(
                catchError(e => throwError(() => new Error(e)))
            );
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
}
