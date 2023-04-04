import { ComponentFixture, TestBed } from '@angular/core/testing';

import { JobPageComponent } from './job-page.component';
import { JobConstants } from '../constants';
import { ComponentsModule } from '../components/components.module';

const job = {
  id: 1,
  name: 'Tester',
  status: JobConstants.STATUS_OPEN,
  applicants: []
};

describe('JobPageComponent', () => {
  let component: JobPageComponent;
  let fixture: ComponentFixture<JobPageComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ ComponentsModule ],
      declarations: [ JobPageComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(JobPageComponent);
    component = fixture.componentInstance;
    component.currentJob = job;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
