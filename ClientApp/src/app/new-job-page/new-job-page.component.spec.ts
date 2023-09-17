/* matts
 * "Matthew's ATS" - Portfolio Project
 * Copyright (C) 2023  Matthew E. Kehrer <matthew@kehrer.dev>
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * You should have received a copy of the GNU Affero General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 **/

import { ComponentFixture, TestBed } from '@angular/core/testing';

import { NewJobPageComponent } from './new-job-page.component';
import { MatCardModule } from '@angular/material/card';
import { FormBuilder, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatInputModule } from '@angular/material/input';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { HttpClientModule } from '@angular/common/http';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { BackendService } from '../services/backend.service';
import { TextFieldModule } from '@angular/cdk/text-field';
import { ComponentsModule } from '../components/components.module';
import { MonitorService } from '../services/monitor.service';

describe('NewJobPageComponent', () => {
  let component: NewJobPageComponent;
  let fixture: ComponentFixture<NewJobPageComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        BrowserAnimationsModule,
        FormsModule,
        ReactiveFormsModule,
        MatInputModule,
        TextFieldModule,
        MatCardModule,
        HttpClientModule,
        HttpClientTestingModule,
        ComponentsModule,
      ],
      declarations: [NewJobPageComponent],
      providers: [
        FormBuilder,
        {
          provide: 'BASE_URL',
          useValue: 'https://localhost/',
        },
        BackendService,
        MonitorService,
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(NewJobPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
