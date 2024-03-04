import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';

import { LoginPageComponent } from './login-page.component';
import { RouterModule } from '@angular/router';
import { RouterTestingModule } from '@angular/router/testing';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatInputModule } from '@angular/material/input';
import { FormBuilder, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { NgbAlertModule } from '@ng-bootstrap/ng-bootstrap';
import { HttpClientModule } from '@angular/common/http';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { User, WSAuthEventTypes, WSAuthMessage, configurationFixure } from '../models';
import { Subject, Subscription, map, of } from 'rxjs';
import { AuthService } from '../services/auth.service';
import { AppModule } from '../app.module';
import { ComponentsModule } from '../components/components.module';
import { MonitorService } from '../services/monitor.service';
import { LoadingBarModule, LoadingBarService } from '@ngx-loading-bar/core';
import { ConfigService } from '../services/config.service';
import { By } from '@angular/platform-browser';
import { ToastService } from '../services/toast.service';

class FakeAuthService {
  constructor() { }

  public subject?: Subject<WSAuthMessage>;

  loginUser = (user?: User) => of('yay!');
  getWSAuthStream = (s2s: Subject<WSAuthMessage>) => this.subject?.pipe(map(m => JSON.stringify(m)));
  terminateWSAuthStream = (c: Subscription, s2s: Subject<WSAuthMessage>) => {
    s2s.complete();
    c.unsubscribe();
  }
}
const fakeAuthService = new FakeAuthService();

describe('LoginPageComponent', () => {
  let component: LoginPageComponent;
  let fixture: ComponentFixture<LoginPageComponent>;
  let toastService: ToastService;

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
        LoadingBarModule,
      ],
      providers: [
        { provide: 'BASE_URL', useValue: '' },
        { provide: AuthService, useValue: fakeAuthService },
        FormBuilder,
        MonitorService,
        LoadingBarService,
        {
          provide: ConfigService,
          useValue: { config: configurationFixure, loadConfig: () => Promise.resolve(configurationFixure) },
        },
        ToastService
      ],
      declarations: [LoginPageComponent],
    }).compileComponents();

    toastService = TestBed.inject(ToastService);

    fixture = TestBed.createComponent(LoginPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('LinkedIn Signup', () => {
    let subbie: Subject<WSAuthMessage>;

    beforeEach(() => {
      subbie = new Subject<WSAuthMessage>();
      fakeAuthService.subject = subbie;
      component.setRegistrationMode(component.REGISTRATION_TYPE_APPLICANT);
      fixture.detectChanges();
    });

    it('should show and hide the message during the process', fakeAsync(() => {
      spyOn(window, 'open');
      spyOn(toastService, 'show').and.stub();

      component.clickApplicantLinkedinButton();
      tick();
      fixture.detectChanges();

      expect(component.retrievingInformation).toBe(true);
      expect(fixture.debugElement.query(By.css('span.please-wait'))).toBeTruthy();

      subbie.next({
        type: WSAuthEventTypes.SERVER_OAUTH_COMPLETED,
        clientIdentity: '123',
        data: {
          fullName: 'Test Testington',
          companyName: null,
          email: 'test@test.com',
          phoneNumber: '5555555555',
        },
      });
      tick();
      fixture.detectChanges();
      tick(1000);
      fixture.detectChanges();

      expect(component.retrievingInformation).toBe(false);
      expect(fixture.debugElement.query(By.css('span.please-wait'))).not.toBeTruthy();

      tick();
      fixture.detectChanges();
      expect(toastService.show).toHaveBeenCalled();
    }));
  });
});
