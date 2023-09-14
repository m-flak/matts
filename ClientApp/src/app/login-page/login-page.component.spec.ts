import { ComponentFixture, TestBed } from '@angular/core/testing';

import { LoginPageComponent } from './login-page.component';
import { RouterModule } from '@angular/router';
import { RouterTestingModule } from '@angular/router/testing';
import { MatExpansionModule } from '@angular/material/expansion';
import {MatInputModule} from '@angular/material/input';
import { FormBuilder, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { NgbAlertModule } from '@ng-bootstrap/ng-bootstrap';
import { HttpClientModule } from '@angular/common/http';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { User } from '../models';
import { of } from 'rxjs';
import { AuthService } from '../services/auth.service';
import { AppModule } from '../app.module';
import { ComponentsModule } from '../components/components.module';
import { MonitorService } from '../services/monitor.service';
import { LoadingBarModule, LoadingBarService } from '@ngx-loading-bar/core';

const FakeAuthService = {
  loginUser: (user?: User) => of('yay!')
}

describe('LoginPageComponent', () => {
  let component: LoginPageComponent;
  let fixture: ComponentFixture<LoginPageComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        AppModule,
        HttpClientModule, 
        HttpClientTestingModule,
        BrowserAnimationsModule,
        FormsModule,
        ReactiveFormsModule,
        RouterModule,
        MatExpansionModule,
        MatInputModule,
        NgbAlertModule,
        RouterTestingModule.withRoutes([]),
        ComponentsModule,
        LoadingBarModule
      ],
      providers: [
        { provide: 'BASE_URL', useValue: '' },
        { provide: AuthService, useValue: FakeAuthService },
        FormBuilder,
        MonitorService,
        LoadingBarService
      ],
      declarations: [LoginPageComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(LoginPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
