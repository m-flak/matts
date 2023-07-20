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
import { HttpClientModule } from "@angular/common/http";
import { HttpTestingController, HttpClientTestingModule } from "@angular/common/http/testing";
import { TestBed } from "@angular/core/testing";
import { JWT_OPTIONS, JwtHelperService, JwtModule } from "@auth0/angular-jwt";
import { jwtOptionsFactory } from "../app.module";
import { AuthService } from "./auth.service";
import { UserRoleConstants } from "../constants";
import { User } from "../models";

// Contains the 'role' claim set to 'employer'
const dummyToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyLCJyb2xlIjoiZW1wbG95ZXIifQ.fZngtAMmz_CZw4XZk0gxifPm37-GmPOdMSzq_cgpcGU";

describe('AuthService', () => {
    let httpMock: HttpTestingController;
    let authService: AuthService;
    let localStorage: any;

    beforeEach(() => {
      TestBed.configureTestingModule({
        declarations: [],
        imports: [
            HttpClientModule,
            HttpClientTestingModule,
            JwtModule.forRoot({})
        ],
        providers: [
            { 
                provide: 'BASE_URL', 
                useValue: 'https://localhost/' 
            },
            {
                provide: JWT_OPTIONS,
                useValue: jwtOptionsFactory('https://localhost/')
            },
            JwtHelperService,
            AuthService
        ]
      });

      localStorage = {};

      spyOn(window.localStorage, 'getItem').and.callFake((key) =>
        key in localStorage ? localStorage[key] : null
      );
      spyOn(window.localStorage, 'setItem').and.callFake(
        (key, value) => (localStorage[key] = value + '')
      );
      spyOn(window.localStorage, 'clear').and.callFake(() => (localStorage = {}));

      authService = TestBed.inject(AuthService);
      httpMock = TestBed.inject(HttpTestingController);
    });

    afterEach(() => {
      localStorage = {};
    });

    it('should instantiate', () => {
        expect(authService).toBeTruthy();
    });

    it('should be return null when unable to get the claim from the token', () => {
      const role: string | null = authService.getLoggedInUserRole();

      expect(role).toBeNull();
    });

    it('should be able to retrieve the role claim for the current token', (done) => {
      const user: User = {
        userName: 'admin',
        password: 'password',
        role: UserRoleConstants.USER_ROLE_EMPLOYER
      };

      authService.loginUser(user).subscribe(() => {
        const role: string | null = authService.getLoggedInUserRole();

        expect(role).toEqual(UserRoleConstants.USER_ROLE_EMPLOYER);
        done();
      });

      const request = httpMock.expectOne('https://localhost/auth/login');
      request.flush(dummyToken);

      httpMock.verify();
    });

    it('should store current user info for the current user', (done) => {
      const user: User = {
        userName: 'admin',
        password: 'password',
        role: UserRoleConstants.USER_ROLE_EMPLOYER
      };

      authService.loginUser(user).subscribe(() => {
        const c_user = authService.currentUser;
        expect(c_user?.userName).toEqual(user.userName);
        expect(c_user?.password).toEqual('');
        expect(c_user?.role).toEqual(user.role);
        done();
      });

      const request = httpMock.expectOne('https://localhost/auth/login');
      request.flush(dummyToken);

      httpMock.verify();
    });

    it('should receive token for login endpoint when logged out', (done) => {
      const user: User = {
        userName: 'admin',
        password: 'password',
        role: UserRoleConstants.USER_ROLE_EMPLOYER
      };

      authService.loginUser(user).subscribe(token => {
        expect(token).toEqual(dummyToken);
        done();
      });

      const request = httpMock.expectOne('https://localhost/auth/login');
      request.flush(dummyToken);

      httpMock.verify();
    });

    it('should receive token for login endpoint when logged in', (done) => {
      localStorage['access_token'] = dummyToken;
      const user: User = {
        userName: 'admin',
        password: 'password',
        role: UserRoleConstants.USER_ROLE_EMPLOYER
      };

      authService.loginUser(user).subscribe(token => {
        expect(token).toEqual(dummyToken);
        done();
      });
    });
});
