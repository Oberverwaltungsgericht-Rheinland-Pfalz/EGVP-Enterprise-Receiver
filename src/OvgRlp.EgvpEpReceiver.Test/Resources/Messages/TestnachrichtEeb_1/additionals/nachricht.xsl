<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:fo="http://www.w3.org/1999/XSL/Format">
  <xsl:output method="html" encoding="UTF-8" doctype-public="-//W3C//DTD HTML 4.0 Transitional//EN" cdata-section-elements="pre listing" indent="yes" />

  <!-- recursive template for replacing \n to <br/> -->
  <xsl:template match="message">
    <xsl:call-template name="escapeNewLine">
      <xsl:with-param name="string" select="." />
    </xsl:call-template>
  </xsl:template>

  <!-- recursive template for replacing \n to <br/> -->
  <xsl:template name="escapeNewLine">
    <xsl:param name="string" />
    <xsl:choose>
      <xsl:when test="contains($string,'&#10;')">
        <xsl:value-of select="substring-before($string,'&#10;')" />
        <br />
        <xsl:call-template name="escapeNewLine">
          <xsl:with-param name="string" select="substring-after($string,'&#10;')" />
        </xsl:call-template>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$string" />
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- recursive template for replacing spaces to  &#160;-->
  <xsl:template name="escapeSpaces">
    <xsl:param name="string" />
    <xsl:choose>
      <xsl:when test="contains($string,'&#32;&#32;')">
        <xsl:value-of select="substring-before($string,'&#32;&#32;')" />&#160; <xsl:call-template name="escapeSpaces">
          <xsl:with-param name="string" select="substring-after($string,'&#32;&#32;')" />
        </xsl:call-template>
      </xsl:when>

      <xsl:otherwise>
        <xsl:value-of select="$string" />
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>


  <xsl:template match="/">

    <html>
      <head>
        <title>nachrichten.html</title>
        <style type="text/css" media="all">
          body { padding: 5px 20px 20px 20px; background: #ffffff; color: #000000; font-family: Verdana, sans-serif; font-size: 9px; } td { font-family: Verdana, sans-serif; font-size: 9px; } td.left { font-weight: bold; color:
          #808080; text-align: right; white-space: nowrap; vertical-align: top;} td.spacer { background-color: #ffffff; } h1 { font-weight: bold; font-size: 16px; } div.message_section {
          background-color: #ffffff; width: 550px; border-width: 1px; border-style: solid; border-color: #808080; } div.sub { font-size: 8px; }
        </style>
      </head>
      <body>
        <h1>Elektronisches Gerichts- und Verwaltungspostfach</h1>

        <div class="message_section">
          <table summary="">
            <tr>
              <td class="left">Nachrichtentyp</td>
              <td>
                <xsl:value-of select="Nachricht/Nachrichtentyp" />
              </td>
            </tr>
            <tr>
              <td class="left">Betreff</td>
              <td width="500px">
                <xsl:value-of select="Nachricht/Betreff" />
              </td>
            </tr>
            <tr>
              <td class="left">Aktenzeichen des Empfängers</td>
              <td width="500px">
                <xsl:value-of select="Nachricht/Aktenzeichen_Empfaenger" />
              </td>
            </tr>
            <tr>
              <td class="left">Aktenzeichen des Absenders</td>
              <td width="500px">
                <xsl:value-of select="Nachricht/Aktenzeichen_Absender" />
              </td>
            </tr>
            <tr>
              <td valign="top" class="left">Nachricht</td>
              <td width="500px">
                <xsl:call-template name="escapeNewLine">
                  <xsl:with-param name="string">
                    <xsl:call-template name="escapeSpaces">
                      <xsl:with-param name="string">
                        <xsl:value-of select="Nachricht/Freitext" />
                      </xsl:with-param>
                    </xsl:call-template>
                  </xsl:with-param>
                </xsl:call-template>
              </td>
            </tr>
          </table>
        </div>
      </body>
    </html>
  </xsl:template>
  <xsl:template match="@version" />
  <xsl:template match="root" />
</xsl:stylesheet>
