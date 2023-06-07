import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { RouterModule } from '@angular/router';
import {MatCardModule} from '@angular/material/card';
import {MatButtonModule} from '@angular/material/button';
import {MatIconModule} from '@angular/material/icon';

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

@NgModule({
  declarations: [
    AppComponent,
    NavMenuComponent,
    HomeComponent,
    JobPageComponent
  ],
  imports: [
    NgbModule,
    NgbAlertModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    ComponentsModule,
    BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
    HttpClientModule,
    FormsModule,
    RouterModule.forRoot([
      { 
        path: '', 
        component: HomeComponent,
        data: { role: UserRoleConstants.USER_ROLE_EMPLOYER },
        canActivate: [ /* TODO: Real Authentication Guard 1st */ HomeGuard ],
        children: [
          { 
            path: 'viewJob/:id', 
            component: JobPageComponent 
          }
        ] 
      }
    ]),
    BrowserAnimationsModule,
    CalendarModule.forRoot({
      provide: DateAdapter,
      useFactory: adapterFactory,
    })
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
