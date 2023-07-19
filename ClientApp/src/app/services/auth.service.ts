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
import { HttpClient } from "@angular/common/http";
import { Injectable, Inject } from "@angular/core";
import { JwtHelperService } from "@auth0/angular-jwt";
import { User } from "../models";
import { Observable, catchError, map, of, tap, throwError } from "rxjs";

@Injectable({
    providedIn: 'root'
})
export class AuthService {
    constructor(
        private http: HttpClient,
        private jwtHelper: JwtHelperService,
        @Inject('BASE_URL') private baseUrl: string
    ) {}

    isLoggedIn(): boolean {
        return ( localStorage.getItem('access_token') !== null );
    }

    loginUser(user: User) : Observable<string> {
        const currentToken: string | null = localStorage.getItem('access_token');
        if (currentToken !== null) {
            return of(currentToken);
        }

        const endpoint = '/auth/login';
        return this.http.post(Location.joinWithSlash(this.baseUrl, endpoint), user)
            .pipe(
                catchError(e => throwError(() => new Error(e))),
                map((t: any) => t || ''),
                tap(token => localStorage.setItem('access_token', token))
            );
    }

    getLoggedInUserRole(): string | null {
        const decodedToken: any = this.jwtHelper.decodeToken();
        let role: string | null = null;

        if (decodedToken !== null && decodedToken.hasOwnProperty('role')) {
            role = (decodedToken.role as string);
        }

        return role;
    }
}
