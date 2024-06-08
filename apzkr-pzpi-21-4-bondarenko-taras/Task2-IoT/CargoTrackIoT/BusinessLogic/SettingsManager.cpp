#include "SettingsManager.h"


void processSetDelayRequest(WiFiClient client, String httpRequest) {
	int newDelayIndex = httpRequest.indexOf("newDelay=");

	if (newDelayIndex != -1) {
		// ����������� ���������� '&' ���� "newDelay="
		int ampersandIndex = httpRequest.indexOf('&', newDelayIndex);

		// ���������� �������� "newDelay" �� �����
		String newDelayString = httpRequest.substring(newDelayIndex + 9, ampersandIndex);

		// ������������ ����� � ���� �����
		int newDelay = newDelayString.toInt();

		if (newDelay > 0) {
			// ������� ��������
			measurementInterval = newDelay;

			// ��������� ����� �������, ��� �� �� POST-�����
			client.println("HTTP/1.1 200 OK");
			client.println("Content-Type: text/html");
			client.println("Connection: close");
			client.println();

			// �������� JavaScript-��� ��� ��������� �������
			client.println("<html><head><script>");
			client.println("window.location.href = window.location.href;");
			client.println("</script></head></html>");
		}
		else {
			// ���� �������� ���������� �������� ��� ���� ��������
			client.println("HTTP/1.1 400 Bad Request");
			client.println("Content-Type: text/html");
			client.println("Connection: close");
			client.println();
			client.println("Invalid delay value");
		}
	}
	else {
		// ���� �� ������� ������ ���������� "newDelay="
		client.println("HTTP/1.1 400 Bad Request");
		client.println("Content-Type: text/html");
		client.println("Connection: close");
		client.println();
		client.println("Missing newDelay parameter");
	}

	client.stop();
}


void processSetPasswordRequest(WiFiClient client, String httpRequest) {
	// �������� ��������� ������ � URL
	String passwordParam = getValue(httpRequest, "password", '&');

	// ��������� ������ ������� ������ � �����
	int spaceIndex = passwordParam.indexOf(' ');
	Serial.println(passwordParam);

	// ³������ ����� �� ������� ������ (�������)
	if (spaceIndex != -1) {
		passwordParam = passwordParam.substring(0, spaceIndex);
	}
	Serial.println(passwordParam);

	// ��������� ������ "�������"
	int equalsIndex = passwordParam.indexOf('=');

	// ³������ ����� �� "�������" (�������)
	if (equalsIndex != -1) {
		passwordParam = passwordParam.substring(equalsIndex + 1);
	}

	// ������� ������
	userPassword = passwordParam;

	// ��������������� �� ������� Settings
	client.print("HTTP/1.1 302 Found\r\n");
	client.print("Location: /settings\r\n");
	client.print("\r\n");
}
void processSetTemperatureUnit(WiFiClient client, String httpRequest) {
	String unitParam = getValue(httpRequest, "selectedUnit", '&');

	// ��������� ������ ������� ������ � �����
	int spaceIndex = unitParam.indexOf(' ');
	Serial.println(unitParam);

	// ³������ ����� �� ������� ������ (�������)
	if (spaceIndex != -1) {
		unitParam = unitParam.substring(0, spaceIndex);
	}
	Serial.println(unitParam);

	// ��������� ������ "�������"
	int equalsIndex = unitParam.indexOf('=');

	// ³������ ����� �� "�������" (�������)
	if (equalsIndex != -1) {
		unitParam = unitParam.substring(equalsIndex + 1);
	}

	Serial.println(unitParam);
	if (unitParam == "Fahrenheit") {
		useFahrenheit = true;
	}
	else {
		useFahrenheit = false;
	}

	// ��������������� �� ������� Settings
	client.print("HTTP/1.1 302 Found\r\n");
	client.print("Location: /settings\r\n");
	client.print("\r\n");
}
