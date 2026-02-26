import { ComponentFixture, TestBed } from '@angular/core/testing';

import { RebalancePreview } from './rebalance-preview';

describe('RebalancePreview', () => {
  let component: RebalancePreview;
  let fixture: ComponentFixture<RebalancePreview>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [RebalancePreview]
    })
    .compileComponents();

    fixture = TestBed.createComponent(RebalancePreview);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
