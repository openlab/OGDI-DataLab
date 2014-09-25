<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:output method="xml" indent="no"/>
  <xsl:template match="/">
    <xsl:text disable-output-escaping="yes">&lt;?xml-stylesheet href="dtbookbasic.css" type="text/css"?&gt;</xsl:text>
    <xsl:text disable-output-escaping="yes">&lt;!DOCTYPE dtbook SYSTEM 'dtbook-2005-3.dtd'&gt;</xsl:text>
    <xsl:text>&#13;&#10;</xsl:text>
    <dtbook version="2005-3" xml:lang="en-US">
      <!--Hard Coded values-->
      <head>
        <meta name="dtb:uid" content="AUTO-UID-5584989674571936019" />
        <meta name="dt:version" content="1.0.0.0" />
        <meta name="dc:Title">
          <xsl:attribute name="content">
            <xsl:value-of select="Root/@tableName"/>
          </xsl:attribute>
        </meta>
        <meta name="dc:Creator" content="Author" />
        <meta name="dc:Identifier" content="AUTO-UID-5584989674571936019" />
        <meta name="dc:Language" content="en-US" />
      </head>
      <book showin="blp">
        <frontmatter>
          <doctitle>
            <xsl:value-of select="Root/@tableName"/>
          </doctitle>
          <docauthor>Author</docauthor>
        </frontmatter>
        <bodymatter id="bodymatter_0001">
          <level1>
            <table border="1">
              <!--Adding the table headers using the first Properties element in the XML-->
              <xsl:apply-templates mode="tableHeading" select="Root/child::properties[1]"/>
              <tbody>
                <!--Taking all the header values seperated by @ into a variable-->
                <xsl:variable name="headerNodes">
                  <xsl:for-each select="Root/child::properties[1]/node()">
                    <xsl:value-of select="concat(position() , name() , '@')"/>
                  </xsl:for-each>
                </xsl:variable>
                <xsl:for-each select="Root/node()">
                  <!--Avoid considering first node for creating table Row, Since first row is for creating Table Headers-->
                  <xsl:if test="position() &gt; 1">
                    <tr>
                      <!--Calling the template to create Table Body-->
                      <xsl:call-template name="tableBody">
                        <xsl:with-param name="varHeaders" select="$headerNodes"/>
                      </xsl:call-template>
                    </tr>
                  </xsl:if>
                </xsl:for-each>
              </tbody>
            </table>
          </level1>
        </bodymatter>
      </book>
    </dtbook>
  </xsl:template>

  <!--Template for retrieving the table header-->
  <xsl:template mode="tableHeading" match="Root/child::properties[1]">
    <thead>
      <tr>
        <xsl:for-each select="./node()">
          <!--Avoid creating headers for entityset and entitykind-->
          <xsl:if test="name()!='entityset' and name()!='entitykind' and name()!='entityid'">
            <th>
              <xsl:value-of select="name()"/>
            </th>
          </xsl:if>
        </xsl:for-each>
      </tr>
    </thead>
  </xsl:template>

  <!--Template for retrieving the table contents -->
  <xsl:template name="tableBody">
    <xsl:param name="varHeaders"/>
    <xsl:param name="varChildNo" select="1"/>
    <xsl:variable name="currentHeader" select="substring-before(substring-after($varHeaders,$varChildNo),'@')"/>
    <xsl:if test="$currentHeader != ''">
      <!--Avoid creating table cell values for entityset and entitykind-->
      <xsl:if test="$currentHeader != 'entityset' and $currentHeader != 'entitykind' and $currentHeader != 'entityid'">
        <td>
          <xsl:choose>
            <xsl:when test="node()[name()=$currentHeader] != ''">
              <xsl:value-of select="concat($currentHeader,' is ',node()[name()=$currentHeader]/node())"/>
            </xsl:when>
            <xsl:otherwise>
              <!--<xsl:value-of select="' '"/>-->
            </xsl:otherwise>
          </xsl:choose>
        </td>
      </xsl:if>
      <!--Calling the template recursively until all the header values are traversed-->
      <xsl:call-template name="tableBody">
        <xsl:with-param name="varHeaders" select="substring-after($varHeaders,'@')"/>
        <xsl:with-param name="varChildNo" select="$varChildNo +1"/>
      </xsl:call-template>
    </xsl:if>
  </xsl:template>
</xsl:stylesheet>

