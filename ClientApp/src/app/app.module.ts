import { BrowserModule } from '@angular/platform-browser';
import { Location } from "@angular/common";
import { APP_INITIALIZER, InjectionToken, NgModule } from '@angular/core';
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
import { HomeComponent, HomeComponentResolver } from './home/home.component';
import { ComponentsModule } from './components/components.module';
import { JobPageComponent } from './job-page/job-page.component';
import { CalendarModule, DateAdapter } from 'angular-calendar';
import { adapterFactory } from 'angular-calendar/date-adapters/date-fns';
import {NgbAlertModule, NgbModule} from '@ng-bootstrap/ng-bootstrap';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { MaterialFileInputModule } from 'ngx-material-file-input';
import { JWT_OPTIONS, JwtModule } from '@auth0/angular-jwt';
import { UserRoleConstants } from './constants';
import { HomeGuard } from './guards/home.guard';
import { AppRootHomeComponent } from './app-root-home';
import { HomeApplicantComponent } from './home-applicant/home-applicant.component';
import { AuthGuard } from './guards/auth.guard';
import { LoginPageComponent } from './login-page/login-page.component';
import { ApplyJobPageComponent } from './apply-job-page/apply-job-page.component';
import { NewJobPageComponent } from './new-job-page/new-job-page.component';
import { MatFormFieldModule } from '@angular/material/form-field';
import { TextFieldModule } from '@angular/cdk/text-field';
import { NgxMaterialTimepickerModule } from 'ngx-material-timepicker';
import { ConfigService } from './services/config.service';
import { PipesModule } from './pipes/pipes.module';
import { LOADING_BAR_CONFIG, LoadingBarModule } from '@ngx-loading-bar/core';
import { LoadingBarHttpClientModule } from '@ngx-loading-bar/http-client';


export const BASE_URL = new InjectionToken<string>('BASE_URL');

@NgModule({
  declarations: [
    AppComponent,
    AppRootHomeComponent,
    NavMenuComponent,
    HomeComponent,
    HomeApplicantComponent,
    JobPageComponent,
    LoginPageComponent,
    ApplyJobPageComponent,
    NewJobPageComponent
  ],
  imports: [
    NgxMaterialTimepickerModule,
    NgbModule,
    NgbAlertModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatExpansionModule,
    MatInputModule,
    TextFieldModule,
    MatFormFieldModule,
    MaterialFileInputModule,
    ComponentsModule,
    PipesModule,
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
            canActivate: [ AuthGuard, HomeGuard ],
          },
          { 
            path: 'employer', 
            component: HomeComponent,
            resolve: { jobList: HomeComponentResolver },
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
            canActivate: [ AuthGuard, HomeGuard ],
            children: [
              { 
                path: 'applyToJob/:id', 
                component: ApplyJobPageComponent 
              }
            ] 
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
    }),
    LoadingBarModule,
    LoadingBarHttpClientModule
  ],
  providers: [ 
    {
      provide: BASE_URL,
      useExisting: 'BASE_URL'
    },
    {
      provide: APP_INITIALIZER,
      multi: true,
      deps: [ConfigService],
      useFactory: (configService: ConfigService) => {
          return () => {
              return configService.loadConfig();
          };
      }
    },
    { provide: LOADING_BAR_CONFIG, useValue: { latencyThreshold: 125 } },
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
      `${Location.joinWithSlash(baseUrl, 'auth')}/`,
      `${Location.joinWithSlash(baseUrl, 'config')}/`
    ]
  };
}
