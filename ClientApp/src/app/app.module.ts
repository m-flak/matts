import { BrowserModule } from '@angular/platform-browser';
import { Location } from '@angular/common';
import { APP_INITIALIZER, InjectionToken, NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { HttpClientModule, HttpClientXsrfModule } from '@angular/common/http';
import { RouterModule } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatInputModule } from '@angular/material/input';

import { AppComponent } from './app.component';
import { NavMenuComponent } from './nav-menu/nav-menu.component';
import { ComponentsModule } from './components/components.module';
import { NgbAlertModule, NgbModule } from '@ng-bootstrap/ng-bootstrap';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { JWT_OPTIONS, JwtModule } from '@auth0/angular-jwt';
import { HomeGuard } from './guards/home.guard';
import { AppRootHomeComponent } from './app-root-home';
import { AuthGuard } from './guards/auth.guard';
import { LoginPageComponent } from './login-page/login-page.component';
import { MatFormFieldModule } from '@angular/material/form-field';
import { TextFieldModule } from '@angular/cdk/text-field';
import { ConfigService } from './services/config.service';
import { PipesModule } from './pipes/pipes.module';
import { LOADING_BAR_CONFIG, LoadingBarModule } from '@ngx-loading-bar/core';
import { LoadingBarHttpClientModule } from '@ngx-loading-bar/http-client';
import { LoadingBarRouterModule } from '@ngx-loading-bar/router';
import { CookieService } from 'ngx-cookie-service';

export const BASE_URL = new InjectionToken<string>('BASE_URL');
export const WS_BASE_URL = new InjectionToken<string>('WS_BASE_URL');

@NgModule({
  declarations: [AppComponent, AppRootHomeComponent, NavMenuComponent, LoginPageComponent],
  imports: [
    NgbModule,
    NgbAlertModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatExpansionModule,
    MatInputModule,
    TextFieldModule,
    MatFormFieldModule,
    ComponentsModule,
    PipesModule,
    BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
    HttpClientModule,
    HttpClientXsrfModule,
    FormsModule,
    ReactiveFormsModule,
    RouterModule.forRoot([
      {
        path: '',
        component: AppRootHomeComponent,
        children: [
          {
            path: '',
            pathMatch: 'full',
            children: [], // Children lets us have an empty component.
            canActivate: [AuthGuard, HomeGuard],
          },
          {
            path: 'employer',
            loadChildren: () => import('./portals/employer/employer-portal.module').then(m => m.EmployerPortalModule),
            canMatch: [AuthGuard],
          },
          {
            path: 'applicant',
            loadChildren: () =>
              import('./portals/applicant/applicant-portal.module').then(m => m.ApplicantPortalModule),
            canMatch: [AuthGuard],
          },
        ],
      },
      {
        path: 'welcome',
        children: [
          {
            path: 'login',
            component: LoginPageComponent,
          },
        ],
      },
    ]),
    BrowserAnimationsModule,
    JwtModule.forRoot({
      jwtOptionsProvider: {
        provide: JWT_OPTIONS,
        deps: [BASE_URL],
        useFactory: jwtOptionsFactory,
      },
    }),
    LoadingBarModule,
    LoadingBarHttpClientModule,
    LoadingBarRouterModule,
  ],
  providers: [
    {
      provide: BASE_URL,
      useExisting: 'BASE_URL',
    },
    {
      provide: WS_BASE_URL,
      useExisting: 'WS_BASE_URL',
    },
    {
      provide: APP_INITIALIZER,
      multi: true,
      deps: [ConfigService],
      useFactory: (configService: ConfigService) => {
        return () => {
          return configService.loadConfig();
        };
      },
    },
    { provide: LOADING_BAR_CONFIG, useValue: { latencyThreshold: 125 } },
    CookieService
  ],
  bootstrap: [AppComponent],
})
export class AppModule {}

export function tokenGetter() {
  return localStorage.getItem('access_token');
}

export function jwtOptionsFactory(baseUrl: string) {
  return {
    tokenGetter: tokenGetter,
    allowedDomains: [baseUrl],
    disallowedRoutes: [`${Location.joinWithSlash(baseUrl, 'auth')}/`, `${Location.joinWithSlash(baseUrl, 'config')}/`],
  };
}
