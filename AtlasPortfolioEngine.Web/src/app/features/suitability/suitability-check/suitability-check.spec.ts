import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SuitabilityCheck } from './suitability-check';

describe('SuitabilityCheck', () => {
  let component: SuitabilityCheck;
  let fixture: ComponentFixture<SuitabilityCheck>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SuitabilityCheck]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SuitabilityCheck);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
