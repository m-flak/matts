import { BrowserModule } from '@angular/platform-browser';
import { Location } from "@angular/common";
import { InjectionToken, NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { RouterModule } from '@angular/router';
import {MatCardModule} from '@angular/material/card';
import {MatButtonModule} from '@angular/material/button';
import {MatIconModule} from '@angular/material/icon';
import {MatExpansionModule} from '@angular/material/expansion';
import {MatInputModule} from '@angular/material/input';

import { AppComponent } from './app.component';
import { NavMenuComponent } from './nav-menu/nav-menu.component';
import { HomeComponent } from './home/home.component';
import { ComponentsModule } from './components/components.module';
import { JobPageComponent } from './job-page/job-page.component';
import { CalendarModule, DateAdapter } from 'angular-calendar';
import { adapterFactory } from 'angular-calendar/date-adapters/date-fns';
import {NgbAlertModule, NgbModule} from '@ng-bootstrap/ng-bootstrap';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { UserRoleConstants } from './constants';
import { HomeGuard } from './guards/home.guard';
import { AppRootHomeComponent } from './app-root-home';
import { HomeApplicantComponent } from './home-applicant/home-applicant.component';
import { AuthGuard } from './guards/auth.guard';
import { LoginPageComponent } from './login-page/login-page.component';
import { JWT_OPTIONS, JwtModule } from '@auth0/angular-jwt';

export const BASE_URL = new InjectionToken<string>('BASE_URL');

@NgModule({
  declarations: [
    AppComponent,
    AppRootHomeComponent,
    NavMenuComponent,
    HomeComponent,
    HomeApplicantComponent,
    JobPageComponent,
    LoginPageComponent
  ],
  imports: [
    NgbModule,
    NgbAlertModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatExpansionModule,
    MatInputModule,
    ComponentsModule,
    BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
    HttpClientModule,
    FormsModule,
    ReactiveFormsModule,
    RouterModule.forRoot([
      {
        path: '',
        component: AppRootHomeComponent,
        children: [
          {
            path: "",
            pathMatch: "full",
            children: [], // Children lets us have an empty component.
            canActivate: [HomeGuard], // Redirects based on role
          },
          { 
            path: 'employer', 
            component: HomeComponent,
            data: { role: UserRoleConstants.USER_ROLE_EMPLOYER },
            canActivate: [ AuthGuard, HomeGuard ],
            children: [
              { 
                path: 'viewJob/:id', 
                component: JobPageComponent 
              }
            ] 
          },
          { 
            path: 'applicant', 
            component: HomeApplicantComponent,
            data: { role: UserRoleConstants.USER_ROLE_APPLICANT },
            canActivate: [ AuthGuard, HomeGuard ]
          }
        ]
      },
      {
        path: 'welcome',
        children: [
          {
            path: 'login',
            component: LoginPageComponent
          }
        ]
      }
    ]),
    BrowserAnimationsModule,
    CalendarModule.forRoot({
      provide: DateAdapter,
      useFactory: adapterFactory,
    }),
    JwtModule.forRoot({
      jwtOptionsProvider: {
        provide: JWT_OPTIONS,
        deps: [ BASE_URL ],
        useFactory: jwtOptionsFactory
      }
    })
  ],
  providers: [ 
    {
      provide: BASE_URL,
      useExisting: 'BASE_URL'
    }
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }

export function tokenGetter() {
  return localStorage.getItem("access_token");
}

export function jwtOptionsFactory(baseUrl: string) {
  return {
    tokenGetter: tokenGetter,
    allowedDomains: [ 
      baseUrl 
    ],
    disallowedRoutes: [
      `${Location.joinWithSlash(baseUrl, 'auth')}/`
    ]
  };
}
