import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class ApiService {
  private readonly baseUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  // Clients
  getClients(): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/clients`);
  }

  getClient(id: string): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/clients/${id}`);
  }

  assessRisk(id: string, answers: number[]): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/clients/${id}/risk-assessment`, answers);
  }

  // Portfolio
  getPortfolio(clientId: string): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/portfolio/${clientId}`);
  }

  getDrift(clientId: string): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/portfolio/${clientId}/drift`);
  }

  // Rebalance
  previewRebalance(clientId: string): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/rebalance/${clientId}/preview`);
  }

  executeRebalance(clientId: string): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/rebalance/${clientId}/execute`, {});
  }

  // Suitability
  checkSuitability(payload: any): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/suitability/check`, payload);
  }
}
