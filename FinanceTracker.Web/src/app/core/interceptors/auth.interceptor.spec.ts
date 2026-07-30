import { TestBed } from '@angular/core/testing';
import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';

import { authInterceptor } from './auth.interceptor';
import { AuthService } from '../services/auth.service';

describe('authInterceptor', () => {
  let httpClient: HttpClient;
  let httpMock: HttpTestingController;
  let authService: { getToken: ReturnType<typeof vi.fn>; logout: ReturnType<typeof vi.fn> };

  beforeEach(() => {
    authService = { getToken: vi.fn(), logout: vi.fn() };

    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([authInterceptor])),
        provideHttpClientTesting(),
        { provide: AuthService, useValue: authService },
      ],
    });

    httpClient = TestBed.inject(HttpClient);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('adds the Authorization header when a token is present', () => {
    authService.getToken.mockReturnValue('abc123');

    httpClient.get('/api/test').subscribe();

    const req = httpMock.expectOne('/api/test');
    expect(req.request.headers.get('Authorization')).toBe('Bearer abc123');
    req.flush({});
  });

  it('does not add the Authorization header when there is no token', () => {
    authService.getToken.mockReturnValue(null);

    httpClient.get('/api/test').subscribe();

    const req = httpMock.expectOne('/api/test');
    expect(req.request.headers.has('Authorization')).toBe(false);
    req.flush({});
  });

  it('logs out automatically on a 401 response', () => {
    authService.getToken.mockReturnValue('abc123');

    httpClient.get('/api/test').subscribe({ error: () => {} });

    const req = httpMock.expectOne('/api/test');
    req.flush({ message: 'Unauthorized' }, { status: 401, statusText: 'Unauthorized' });

    expect(authService.logout).toHaveBeenCalled();
  });

  it('does not log out on non-401 error responses', () => {
    authService.getToken.mockReturnValue('abc123');

    httpClient.get('/api/test').subscribe({ error: () => {} });

    const req = httpMock.expectOne('/api/test');
    req.flush({ message: 'Server error' }, { status: 500, statusText: 'Internal Server Error' });

    expect(authService.logout).not.toHaveBeenCalled();
  });
});
