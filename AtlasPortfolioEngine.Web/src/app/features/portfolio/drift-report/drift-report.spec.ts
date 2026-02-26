import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DriftReport } from './drift-report';

describe('DriftReport', () => {
  let component: DriftReport;
  let fixture: ComponentFixture<DriftReport>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DriftReport]
    })
    .compileComponents();

    fixture = TestBed.createComponent(DriftReport);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
