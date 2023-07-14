import { ComponentFixture, TestBed } from '@angular/core/testing';

import { HomeApplicantComponent } from './home-applicant.component';

describe('HomeApplicantComponent', () => {
  let component: HomeApplicantComponent;
  let fixture: ComponentFixture<HomeApplicantComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ HomeApplicantComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(HomeApplicantComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
