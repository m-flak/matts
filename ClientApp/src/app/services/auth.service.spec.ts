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
import { Server } from 'mock-socket';
import { Location } from '@angular/common';
import { HttpClientModule, HttpResponse } from '@angular/common/http';
import { HttpTestingController, HttpClientTestingModule } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { JWT_OPTIONS, JwtHelperService, JwtModule } from '@auth0/angular-jwt';
import { jwtOptionsFactory } from '../app.module';
import { AuthService } from './auth.service';
import { UserRoleConstants } from '../constants';
import { User, UserRegistration, WSAuthEventTypes, WSAuthMessage } from '../models';
import { Subject, Subscription, map, toArray } from 'rxjs';
import { CookieService } from 'ngx-cookie-service';

// Contains the 'role' claim set to 'employer'
const DUMMY_TOKEN =
  'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJhZG1pbiIsIm5hbWUiOiJKb2huIERvZSIsImlhdCI6MTUxNjIzOTAyMiwicm9sZSI6ImVtcGxveWVyIn0.2sqxyiZcJHaLkSmBTTPu3gUqXEpJJKGJxNJi3_OuxrQ';

const WEB_SOCKET_URL = 'ws://localhost:44475/';

describe('AuthService', () => {
  let websocketServer: Server;
  let httpMock: HttpTestingController;
  let authService: AuthService;
  let cookieService: CookieService;
  let localStorage: any;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [],
      imports: [HttpClientModule, HttpClientTestingModule, JwtModule.forRoot({})],
      providers: [
        {
          provide: 'BASE_URL',
          useValue: 'https://localhost/',
        },
        {
          provide: 'WS_BASE_URL',
          useValue: WEB_SOCKET_URL,
        },
        {
          provide: JWT_OPTIONS,
          useValue: jwtOptionsFactory('https://localhost/'),
        },
        JwtHelperService,
        CookieService,
        AuthService,
      ],
    });

    localStorage = {};

    spyOn(window.localStorage, 'getItem').and.callFake(key => (key in localStorage ? localStorage[key] : null));
    spyOn(window.localStorage, 'setItem').and.callFake((key, value) => (localStorage[key] = value + ''));
    spyOn(window.localStorage, 'clear').and.callFake(() => (localStorage = {}));

    authService = TestBed.inject(AuthService);
    cookieService = TestBed.inject(CookieService);
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

  it('should be able to retrieve the role claim for the current token', done => {
    const user: User = {
      userName: 'admin',
      password: 'password',
      role: UserRoleConstants.USER_ROLE_EMPLOYER,
    };

    authService.loginUser(user).subscribe(() => {
      const role: string | null = authService.getLoggedInUserRole();

      expect(role).toEqual(UserRoleConstants.USER_ROLE_EMPLOYER);
      done();
    });

    const request = httpMock.expectOne('https://localhost/api/v1/auth/login');
    request.flush(DUMMY_TOKEN);

    httpMock.verify();
  });

  it('should store current user info for the current user', done => {
    const user: User = {
      userName: 'admin',
      password: 'password',
      role: UserRoleConstants.USER_ROLE_EMPLOYER,
    };

    authService.loginUser(user).subscribe(() => {
      const c_user = authService.currentUser;
      expect(c_user?.userName).toEqual(user.userName);
      expect(c_user?.password).toEqual('');
      expect(c_user?.role).toEqual(user.role);
      done();
    });

    const request = httpMock.expectOne('https://localhost/api/v1/auth/login');
    request.flush(DUMMY_TOKEN);

    httpMock.verify();
  });

  it('should receive token for login endpoint when logged out', done => {
    const user: User = {
      userName: 'admin',
      password: 'password',
      role: UserRoleConstants.USER_ROLE_EMPLOYER,
    };

    authService.loginUser(user).subscribe(token => {
      expect(token).toEqual(DUMMY_TOKEN);
      done();
    });

    const request = httpMock.expectOne('https://localhost/api/v1/auth/login');
    request.flush(DUMMY_TOKEN);

    httpMock.verify();
  });

  it('should receive token for login endpoint when logged in', done => {
    localStorage['access_token'] = DUMMY_TOKEN;
    const user: User = {
      userName: 'admin',
      password: 'password',
      role: UserRoleConstants.USER_ROLE_EMPLOYER,
    };

    authService.loginUser(user).subscribe(token => {
      expect(token).toEqual(DUMMY_TOKEN);
      done();
    });
  });

  it('should register a new user', done => {
    const newUser: UserRegistration = {
      companyName: 'Test Co.',
      fullName: 'Test Testington',
      email: 'test@test.com',
      phoneNumber: '555-555-5555',
      userName: 'ttest123',
      password: 'testingrocks',
      role: UserRoleConstants.USER_ROLE_EMPLOYER,
    };

    authService.registerUser(newUser).subscribe(response => {
      expect(response.status).toEqual(200);
      done();
    });

    const request = httpMock.expectOne('https://localhost/api/v1/auth/register');
    request.flush(null, { status: 200, statusText: 'OK' });

    httpMock.verify();
  });

  describe('OAuth Flow Websocket Tests', () => {
    let clientIdentity = '';
    let secondRequest = false;

    beforeEach(() => {
      cookieService.set('XSRF-TOKEN', '123');
      secondRequest = false;
      websocketServer = new Server(Location.joinWithSlash(WEB_SOCKET_URL, '/api/v1/ws/oauth/linkedin'));
      websocketServer.on('connection', socket => {
        socket.send(
          JSON.stringify({
            type: WSAuthEventTypes.SERVER_CONNECTION_ESTABLISHED,
            clientIdentity: '',
            data: null,
          }),
        );
        socket.on('message', message => {
          if (typeof message !== 'string') {
            return;
          }
          const parsedMsg: WSAuthMessage = JSON.parse(message);
          if (parsedMsg.type === WSAuthEventTypes.CLIENT_OAUTH_START) {
            clientIdentity = parsedMsg.clientIdentity;
            const reply: WSAuthMessage = {
              type: WSAuthEventTypes.SERVER_OAUTH_STARTED,
              clientIdentity: clientIdentity,
              data: null,
            };
            socket.send(JSON.stringify(reply));
          } else if (parsedMsg.type === WSAuthEventTypes.CLIENT_OAUTH_REQUEST_STATUS) {
            if (secondRequest === false) {
              const reply: WSAuthMessage = {
                type: WSAuthEventTypes.SERVER_OAUTH_PENDING,
                clientIdentity: clientIdentity,
                data: null,
              };
              socket.send(JSON.stringify(reply));
              secondRequest = true;
            } else {
              const theData: UserRegistration = {
                fullName: 'John Doe',
                companyName: null,
                email: 'john.doe@gmail.com',
                phoneNumber: '555-555-5454',
                userName: '',
                password: '',
                role: '',
              };
              const reply: WSAuthMessage = {
                type: WSAuthEventTypes.SERVER_OAUTH_COMPLETED,
                clientIdentity: clientIdentity,
                data: theData,
              };
              socket.send(JSON.stringify(reply));
            }
          }
        });
      });
    });

    it('waits and retrieves the user profile information claims', done => {
      spyOn(cookieService, 'get').and.callThrough();

      const sendToSocket = new Subject<WSAuthMessage>();
      const socketMessages = authService.getWSAuthStream(sendToSocket);

      let sub: Subscription = socketMessages.subscribe(jsonMsg => {
        const message: WSAuthMessage = JSON.parse(jsonMsg);
        if (message) {
          if (message.type === WSAuthEventTypes.SERVER_CONNECTION_ESTABLISHED) {
            authService.sendWSAuthStart(sendToSocket);
            expect(cookieService.get).toHaveBeenCalled();
          } else if (
            message.type === WSAuthEventTypes.SERVER_OAUTH_STARTED ||
            message.type === WSAuthEventTypes.SERVER_OAUTH_PENDING
          ) {
            expect(clientIdentity).toEqual('123');
            const reply: WSAuthMessage = {
              type: WSAuthEventTypes.CLIENT_OAUTH_REQUEST_STATUS,
              clientIdentity: clientIdentity,
              data: null,
            };
            sendToSocket.next(reply);
          } else if (message.type === WSAuthEventTypes.SERVER_OAUTH_COMPLETED) {
            expect(message.data).not.toBeNull();
            expect(message.data).toBeDefined();
            const theData: UserRegistration = message.data;
            expect(theData.fullName).toEqual('John Doe');
            expect(theData.email).toEqual('john.doe@gmail.com');
            expect(theData.phoneNumber).toEqual('555-555-5454');
            authService.terminateWSAuthStream(sub, sendToSocket);
            done();
          }
        }
      });
    });

    afterEach(() => {
      websocketServer.close();
      clientIdentity = '';
    });
  });
});
