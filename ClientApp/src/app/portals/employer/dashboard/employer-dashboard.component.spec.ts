import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EmployerDashboardComponent } from './employer-dashboard.component';
import { MatCardModule } from '@angular/material/card';
import { ComponentsModule } from 'src/app/components/components.module';
import { MatTableModule } from '@angular/material/table';

describe('EmployerDashboardComponent', () => {
  let component: EmployerDashboardComponent;
  let fixture: ComponentFixture<EmployerDashboardComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MatCardModule, MatTableModule, ComponentsModule],
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
