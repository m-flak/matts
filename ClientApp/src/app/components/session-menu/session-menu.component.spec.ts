import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SessionMenuComponent } from './session-menu.component';
import { ComponentsModule } from '../components.module';

describe('SessionMenuComponent', () => {
  let component: SessionMenuComponent;
  let fixture: ComponentFixture<SessionMenuComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ ComponentsModule ],
      declarations: [ SessionMenuComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SessionMenuComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
