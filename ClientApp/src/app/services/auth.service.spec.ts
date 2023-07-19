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

    it('should instantiate', () => {
        expect(authService).toBeTruthy();
    });
});
