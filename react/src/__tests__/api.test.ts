import { describe, it, expect, beforeEach } from 'vitest';
import { AxiosHeaders } from 'axios';
import api from '../services/api';

describe('API Request Interceptor', () => {
  beforeEach(() => {
    localStorage.clear();
    document.cookie = 'XSRF-TOKEN=; Max-Age=0; path=/';
    document.cookie = 'RequestVerificationToken=; Max-Age=0; path=/';
  });

  it('sets RequestVerificationToken and Authorization headers when cookie and token are present', async () => {
    document.cookie = 'XSRF-TOKEN=test-xsrf-token-123; path=/';
    localStorage.setItem('access_token', 'mock-bearer-token');

    const mockConfig: any = {
      headers: new AxiosHeaders(),
      method: 'post',
      url: '/api/app/games/test/guess'
    };

    const interceptor = (api.interceptors.request as any).handlers[0];
    const transformedConfig = await interceptor.fulfilled(mockConfig);

    expect(transformedConfig.headers.get('Authorization')).toBe('Bearer mock-bearer-token');
    expect(transformedConfig.headers.get('RequestVerificationToken')).toBe('test-xsrf-token-123');
    expect(transformedConfig.headers.get('X-XSRF-TOKEN')).toBe('test-xsrf-token-123');
    expect(transformedConfig.headers.get('X-Requested-With')).toBe('XMLHttpRequest');
    expect(transformedConfig.headers.get('X-Correlation-Id')).toBeDefined();
  });

  it('reads RequestVerificationToken cookie as fallback if XSRF-TOKEN is not set', async () => {
    document.cookie = 'RequestVerificationToken=fallback-rv-token; path=/';

    const mockConfig: any = {
      headers: new AxiosHeaders(),
      method: 'post',
      url: '/api/app/games/test/guess'
    };

    const interceptor = (api.interceptors.request as any).handlers[0];
    const transformedConfig = await interceptor.fulfilled(mockConfig);

    expect(transformedConfig.headers.get('RequestVerificationToken')).toBe('fallback-rv-token');
    expect(transformedConfig.headers.get('X-XSRF-TOKEN')).toBe('fallback-rv-token');
  });
});

