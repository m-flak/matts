import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EmployerDashboardComponent } from './employer-dashboard.component';
import { MatCardModule } from '@angular/material/card';

describe('EmployerDashboardComponent', () => {
  let component: EmployerDashboardComponent;
  let fixture: ComponentFixture<EmployerDashboardComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MatCardModule],
      declarations: [EmployerDashboardComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(EmployerDashboardComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
