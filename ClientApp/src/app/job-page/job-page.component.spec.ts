import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import {MatButtonModule} from '@angular/material/button';

import { JobPageComponent } from './job-page.component';
import { JobConstants } from '../constants';
import { ComponentsModule } from '../components/components.module';
import { ActivatedRoute, ParamMap } from '@angular/router';
import { of } from 'rxjs';
import { BackendService } from '../services/backend.service';

const job = {
  id: 1,
  name: 'Tester',
  status: JobConstants.STATUS_OPEN,
  applicants: []
};

const FakeBackendService = {
  getJobDetails: (id: number) => of(job)
};

describe('JobPageComponent', () => {
  let component: JobPageComponent;
  let fixture: ComponentFixture<JobPageComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ ComponentsModule, MatButtonModule ],
      declarations: [ JobPageComponent ],
      providers: [
        { provide: BackendService, useValue: FakeBackendService },
        { provide: ActivatedRoute, useValue: { 'paramMap': of((() => { let m = new Map(); m.set('id', '1'); return m as unknown as ParamMap;})()) } }
      ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(JobPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load the job details', fakeAsync(() => {
    tick(2);
    expect(component.currentJob).not.toBeNull();
    expect(component.currentJob?.id).toEqual(job.id);
    expect(component.currentJob?.name).toEqual(job.name);
    expect(component.currentJob?.status).toEqual(job.status);
  }));
});
